#!/usr/bin/env python3
"""
Generates ../30-Verify-MasterData.sql: every master-data key the C# code looks up
(StatusKeys.X, "s_some_status" literals, role names in [Authorize] / IsInRole),
with the table it is looked up in, so a DB can be checked for rows the code
needs but the database lacks. A missing row usually means a screen or workflow
step that fails with "Sequence contains no elements" or a NullReferenceException.

Usage (from the repo root):  python3 Deployment/Database/tools/generate_masterdata_check.py
"""
import glob, os, re, sys
from collections import defaultdict

sys.path.insert(0, os.path.dirname(__file__))
import generate_schema_check as chk  # noqa: E402

OUT = os.path.join(chk.ROOT, 'Deployment', 'Database', '30-Verify-MasterData.sql')


def strip(s):
    s = re.sub(r'/\*.*?\*/', '', s, flags=re.S)
    return re.sub(r'(?<![:"])//[^\n]*', '', s)


def main():
    files = [f for f in glob.glob(os.path.join(chk.MVC, '**', '*.cs'), recursive=True)
             if not re.search(r'[\\/](Tests|Migrations|obj|bin)[\\/]', f)]
    src = {f: strip(open(f, encoding='utf-8-sig', errors='ignore').read()) for f in files}
    allsrc = '\n'.join(src.values())

    # constants: class -> {name: value}
    consts = defaultdict(dict)
    for s in src.values():
        for cm in re.finditer(r'class\s+(\w+)[^{]*\{', s):
            depth, i = 1, cm.end()
            while depth and i < len(s):
                depth += {'{': 1, '}': -1}.get(s[i], 0)
                i += 1
            for name, val in re.findall(r'const\s+string\s+(\w+)\s*=\s*"([^"]*)"', s[cm.end():i]):
                consts[cm.group(1)].setdefault(name, val)

    # DbSet property -> table
    _, edmx = chk.latest_snapshot()
    ssdl = edmx[edmx.find('<StorageModels>'):edmx.find('</StorageModels>')]
    mapping = edmx[edmx.find('<Mappings>'):edmx.find('</Mappings>')]
    set_table = dict(re.findall(r'<EntitySet Name="([^"]+)" EntityType="[^"]*" Schema="\w+" (?:\w+:)?Table="([^"]+)"', ssdl))
    ent_table = {e: set_table.get(st) for e, st in re.findall(r'<EntityTypeMapping TypeName="[^"]*?\.([^".]+)">\s*<MappingFragment StoreEntitySet="([^"]+)"', mapping)}
    ctx = strip(open(os.path.join(chk.MVC, 'DataAccessLayer', 'eServicesDbContext.cs'), encoding='utf-8-sig').read())
    dbset_table = {}
    for etype, prop in re.findall(r'public\s+(?:virtual\s+)?(?:I?Db)Set<([\w.]+)>\s+(\w+)', ctx):
        t = ent_table.get(etype.split('.')[-1])
        if t:
            dbset_table[prop] = t

    need = defaultdict(set)   # table -> keys
    pat = re.compile(r'\.(\w+)\s*\.\s*(?:Where|First|FirstOrDefault|Single|SingleOrDefault|Any|Count|Last|LastOrDefault)(?:Async)?\(\s*(\w+)\s*=>\s*([^;]{0,400}?)\)\s*[;.)]', re.S)
    for m in pat.finditer(allsrc):
        dbset, v, body = m.groups()
        table = dbset_table.get(dbset)
        if not table:
            continue
        for k in re.findall(r'\b' + v + r'\.Key\s*==\s*"([^"]+)"', body):
            need[table].add(k)
        for cls, name in re.findall(r'\b' + v + r'\.Key\s*==\s*(\w+)\.(\w+)', body):
            if name in consts.get(cls, {}):
                need[table].add(consts[cls][name])
    # Constants passed to helper methods (GetStatusId(StatusKeys.X) etc.) map by their class.
    class_table = {'StatusKeys': 'Status', 'StatusTypeKeys': 'StatusTypes', 'RCSActionTypeKeys': 'RCSActionTypes',
                   'ActivityTrackerMessageKeys': 'ActivityTrackerMessages', 'DocumentTypeKeys': 'DocumentTypes',
                   'ResponsibilityTypeKeys': 'ResponsibilityTypes', 'EmailContentKeys': 'EmailContentTypes',
                   'ReferenceTypeKeys': 'ReferenceTypes', 'ComplaintStatusKeys': 'Status',
                   'PaymentTransgressionStatusKeys': 'Status', 'ServiceRequestStatusKeys': 'Status',
                   'ApplicationKeys': 'Applications'}
    for cls, table in class_table.items():
        for name, val in consts.get(cls, {}).items():
            if re.search(r'\b%s\.%s\b' % (cls, name), allsrc):
                need[table].add(val)

    # Roles
    roles = set()
    for r in re.findall(r'Authorize\s*\(\s*Roles\s*=\s*"([^"]+)"', allsrc):
        roles.update(x.strip() for x in r.split(','))
    roles.update(re.findall(r'IsInRole\(\s*"([^"]+)"\s*\)', allsrc))
    roles.update(re.findall(r'AddToRoleAsync?\(\s*[^,]+,\s*"([^"]+)"', allsrc))
    roles.discard('')

    rows = [(t, k) for t in sorted(need) for k in sorted(need[t]) if k]
    lines = ["  (N'%s', N'%s')" % (t, k.replace("'", "''")) for t, k in rows]
    lines += ["  (N'AspNetRoles', N'%s')" % r.replace("'", "''") for r in sorted(roles)]
    inserts = '\n'.join('INSERT @need (TableName, KeyValue) VALUES\n' + ',\n'.join(lines[i:i + 900]) + ';'
                        for i in range(0, len(lines), 900))
    tables = sorted(set(need) | {'AspNetRoles'})
    colset = {(t, c) for _, t, c in chk.ssdl_columns(edmx)[0]}
    checks = '\nUNION ALL\n'.join(
        ("SELECT n.TableName, n.KeyValue FROM @need n WHERE n.TableName = N'{t}' AND NOT EXISTS "
         "(SELECT 1 FROM dbo.[{t}] x WHERE x.[{c}] = n.KeyValue)").format(t=t, c='Name' if t == 'AspNetRoles' else 'Key')
        for t in tables)
    inactive = '\nUNION ALL\n'.join(
        ("SELECT n.TableName, n.KeyValue FROM @need n JOIN dbo.[{t}] x ON x.[Key] = n.KeyValue "
         "WHERE n.TableName = N'{t}' AND (x.IsActive = 0 OR x.IsDeleted = 1)").format(t=t)
        for t in tables if t != 'AspNetRoles' and {(t, 'IsActive'), (t, 'IsDeleted')} <= colset)
    sql = f"""/*
  30-Verify-MasterData.sql  (GENERATED by tools/generate_masterdata_check.py, do not hand-edit)
  Lists master-data rows the code looks up by key but the database does not have.

    sqlcmd -S <server> -E -W -i 30-Verify-MasterData.sql -v DbName="PropertyLeaseManagementRealEstateFinal"

  Result 1: missing rows (empty = good). Result 2: rows present but inactive/deleted.
  Keys are collected from the whole code base, so a few may belong to modules that
  EHC testing never opens (rates rebate, incentive policy, RCS); judge by the key name.
*/
SET NOCOUNT ON;
USE [$(DbName)];
DECLARE @need TABLE (TableName sysname, KeyValue nvarchar(256));
{inserts}

SELECT TableName, KeyValue AS MissingKey FROM (
{checks}
) m ORDER BY TableName, MissingKey;

SELECT TableName, KeyValue AS InactiveOrDeletedKey FROM (
{inactive}
) m ORDER BY TableName, InactiveOrDeletedKey;
"""
    open(OUT, 'w', encoding='utf-8', newline='\r\n').write(sql)
    print(f'{OUT}: ' + ', '.join(f'{t} {len(need[t])}' for t in sorted(need)) + f', roles {len(roles)}')


if __name__ == '__main__':
    main()
