#!/usr/bin/env python3
"""
Generates ../90-Verify-Schema.sql: the list of tables and columns the code
expects in the main PLM database (eServicesDbContext).

Sources
  1. The EF model snapshot stored in the newest Migrations/*.resx ("Target"),
     i.e. everything the code-first migrations know about.
  2. Entities that have a DbSet but are NOT in that snapshot (they were added
     with hand-written SQL scripts, e.g. the RE_* Real Estate tables). Their
     columns are read from the C# model's simple public properties, so treat
     those rows as a strong hint rather than gospel.

Usage (from the repo root):  python3 Deployment/Database/tools/generate_schema_check.py
"""
import base64, glob, gzip, os, re, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
MVC = os.path.join(ROOT, 'C8.eServices.Mvc')
OUT = os.path.join(ROOT, 'Deployment', 'Database', '90-Verify-Schema.sql')

SIMPLE = {'int', 'long', 'short', 'byte', 'bool', 'decimal', 'double', 'float', 'string',
          'DateTime', 'DateTimeOffset', 'TimeSpan', 'Guid', 'byte[]'}


def latest_snapshot():
    resx = sorted(glob.glob(os.path.join(MVC, 'Migrations', '*.resx')))[-1]
    s = open(resx, encoding='utf-8-sig').read()
    blob = re.search(r'<data name="Target".*?<value>(.*?)</value>', s, re.S).group(1)
    return os.path.basename(resx)[:-5], gzip.decompress(base64.b64decode(blob)).decode('utf-8')


def ssdl_columns(edmx):
    ssdl = edmx[edmx.find('<StorageModels>'):edmx.find('</StorageModels>')]
    types = {}
    for m in re.finditer(r'<EntityType Name="([^"]+)"[^>]*>(.*?)</EntityType>', ssdl, re.S):
        types[m.group(1)] = re.findall(r'<Property Name="([^"]+)"', m.group(2))
    rows, tables = [], set()
    for m in re.finditer(r'<EntitySet Name="([^"]+)" EntityType="[^"]*?\.([^".]+)" Schema="(\w+)" (?:\w+:)?Table="([^"]+)"', ssdl):
        _, etype, schema, table = m.groups()
        if table == '__MigrationHistory':
            continue
        tables.add(table)
        rows += [(schema, table, c) for c in types.get(etype, [])]
    conceptual = edmx[edmx.find('<ConceptualModels>'):edmx.find('</ConceptualModels>')]
    known = set(re.findall(r'<EntityType Name="([^"]+)"', conceptual))
    return rows, tables, known


def find_class(name):
    for path in glob.glob(os.path.join(MVC, 'Models', '**', '*.cs'), recursive=True):
        src = open(path, encoding='utf-8-sig', errors='ignore').read()
        src = re.sub(r'/\*.*?\*/', '', src, flags=re.S)
        src = re.sub(r'(?<![:"])//[^\n]*', '', src)      # strip comments, keep "http://" in strings
        m = re.search(r'(\[Table\("([^"]+)"\)\]\s*)?public\s+(?:partial\s+)?class\s+' + re.escape(name) + r'\b[^{]*\{', src)
        if m:
            return src, m
    return None, None


def class_columns(name):
    src, m = find_class(name)
    if not src:
        return None, []
    table = m.group(2)
    depth, i = 1, m.end()
    while depth and i < len(src):
        depth += {'{': 1, '}': -1}.get(src[i], 0)
        i += 1
    body = src[m.end():i]
    cols = []
    for pm in re.finditer(r'((?:\[[^\]]*\]\s*)*)public\s+(?!virtual)([\w\[\]?.<>]+)\s+(\w+)\s*\{\s*get;', body):
        attrs, ptype, pname = pm.groups()
        if 'NotMapped' in attrs:
            continue
        base = ptype.rstrip('?')
        if base in SIMPLE or base.startswith('System.'):
            cm = re.search(r'Column\("([^"]+)"', attrs)
            cols.append(cm.group(1) if cm else pname)
    return table, cols


def main():
    mig, edmx = latest_snapshot()
    rows, tables, known = ssdl_columns(edmx)
    ctx = open(os.path.join(MVC, 'DataAccessLayer', 'eServicesDbContext.cs'), encoding='utf-8-sig').read()
    ctx = re.sub(r'/\*.*?\*/', '', ctx, flags=re.S)      # drop block comments
    ctx = re.sub(r'//[^\n]*', '', ctx)                    # drop commented-out DbSets
    extra = []
    for etype, prop in re.findall(r'public\s+(?:virtual\s+)?(?:I?Db)Set<([\w.]+)>\s+(\w+)', ctx):
        etype = etype.split('.')[-1]
        if etype in known:
            continue
        table, cols = class_columns(etype)
        table = table or prop          # EF default would pluralise; DbSet name is the usual match here
        if table in tables:
            continue
        tables.add(table)
        extra.append(table)
        rows += [('dbo', table, c) for c in cols]

    # Columns the C# models have that the snapshot lacks (added later by hand SQL).
    mapping = edmx[edmx.find('<Mappings>'):edmx.find('</Mappings>')]
    ssdl = edmx[edmx.find('<StorageModels>'):edmx.find('</StorageModels>')]
    set_table = dict(re.findall(r'<EntitySet Name="([^"]+)" EntityType="[^"]*" Schema="\w+" (?:\w+:)?Table="([^"]+)"', ssdl))
    have = {(t, c) for _, t, c in rows}
    for entity, eset in re.findall(r'<EntityTypeMapping TypeName="[^"]*?\.([^".]+)">\s*<MappingFragment StoreEntitySet="([^"]+)"', mapping):
        t = set_table.get(eset)
        if not t:
            continue
        for c in class_columns(entity)[1]:
            if (t, c) not in have:
                have.add((t, c))
                rows.append(('dbo', t, c))

    lines = ["  (N'%s', N'%s', N'%s')" % r for r in rows]
    inserts = '\n'.join(
        'INSERT @expected (SchemaName, TableName, ColumnName) VALUES\n' + ',\n'.join(lines[i:i + 900]) + ';'
        for i in range(0, len(lines), 900))
    sql = f"""/*
  90-Verify-Schema.sql  (GENERATED by tools/generate_schema_check.py, do not hand-edit)
  Compares a restored PLM main database with what the code on this branch expects.

    sqlcmd -S <server\\instance> -E -i 90-Verify-Schema.sql -v DbName="PropertyLeaseManagementRealEstate"

  Output
    1. Summary: latest EF migration in the DB vs. in the code, table/column counts.
       MigrationState is informational only: 10-Upgrade-Schema.sql adds what is missing
       without writing __MigrationHistory, and the app never checks it (SetInitializer(null)).
    2. Missing tables.
    3. Missing columns on tables that do exist.
  Empty result sets 2 and 3 mean the schema matches. Extra tables/columns in the
  DB are ignored (they do no harm to EF).

  Expected latest code migration: {mig}
  Tables added outside EF migrations (checked from C# models): {', '.join(sorted(extra)) or 'none'}
*/
SET NOCOUNT ON;
USE [$(DbName)];

DECLARE @expected TABLE (SchemaName sysname, TableName sysname, ColumnName sysname);
{inserts}

DECLARE @dbMigration nvarchar(150) = NULL;
IF OBJECT_ID(N'dbo.__MigrationHistory', N'U') IS NOT NULL
    SELECT TOP (1) @dbMigration = MigrationId FROM dbo.__MigrationHistory
    WHERE ContextKey = N'C8.eServices.Mvc.DataAccessLayer.eServicesDbContext' ORDER BY MigrationId DESC;

SELECT DB_NAME() AS DatabaseName,
       @dbMigration AS LatestMigrationInDb,
       N'{mig}' AS LatestMigrationInCode,
       CASE WHEN @dbMigration = N'{mig}' THEN 'MATCH'
            WHEN @dbMigration > N'{mig}' THEN 'DB AHEAD OF CODE'
            WHEN @dbMigration IS NULL THEN 'NO MIGRATION HISTORY'
            ELSE 'DB BEHIND CODE' END AS MigrationState,
       (SELECT COUNT(DISTINCT TableName) FROM @expected) AS ExpectedTables,
       (SELECT COUNT(*) FROM @expected) AS ExpectedColumns;

SELECT DISTINCT e.SchemaName + '.' + e.TableName AS MissingTable
FROM @expected e
WHERE OBJECT_ID(QUOTENAME(e.SchemaName) + '.' + QUOTENAME(e.TableName), 'U') IS NULL
ORDER BY 1;

SELECT e.SchemaName + '.' + e.TableName AS TableName, e.ColumnName AS MissingColumn
FROM @expected e
WHERE OBJECT_ID(QUOTENAME(e.SchemaName) + '.' + QUOTENAME(e.TableName), 'U') IS NOT NULL
  AND COL_LENGTH(QUOTENAME(e.SchemaName) + '.' + QUOTENAME(e.TableName), e.ColumnName) IS NULL
ORDER BY 1, 2;
"""
    open(OUT, 'w', encoding='utf-8', newline='\r\n').write(sql)
    print(f'{OUT}: {len(tables)} tables, {len(rows)} columns, snapshot {mig}, extra: {extra}')


if __name__ == '__main__':
    sys.exit(main())
