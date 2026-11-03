#!/usr/bin/env python3
"""
Generates ../10-Upgrade-Schema.sql: an idempotent script that brings any PLM main
database up to the schema the code on this branch expects. It only ADDS things:
missing tables (with primary keys, indexes and foreign keys) and missing columns.
It never drops, renames or narrows anything, so it is safe on a DB that is ahead.

Sources, in order of trust
  1. The EF model snapshot in the newest Migrations/*.resx (exact SQL types).
  2. C# model classes, for tables and columns added later with hand-written SQL
     (RE_*, AuditTrail_*, LeaseTerminations refund/vacating fields, ...).

Usage (from the repo root):  python3 Deployment/Database/tools/generate_schema_upgrade.py
"""
import os, re, sys

sys.path.insert(0, os.path.dirname(__file__))
import generate_schema_check as chk  # noqa: E402

OUT = os.path.join(chk.ROOT, 'Deployment', 'Database', '10-Upgrade-Schema.sql')

CS_TYPES = {
    'int': 'int', 'long': 'bigint', 'short': 'smallint', 'byte': 'tinyint', 'bool': 'bit',
    'decimal': 'decimal(18, 2)', 'double': 'float', 'float': 'real', 'DateTime': 'datetime',
    'DateTimeOffset': 'datetimeoffset', 'TimeSpan': 'time', 'Guid': 'uniqueidentifier',
    'byte[]': 'varbinary(max)', 'string': 'nvarchar(max)',
}
DEFAULTS = {'bit': '0', 'int': '0', 'bigint': '0', 'smallint': '0', 'tinyint': '0', 'float': '0', 'real': '0',
            'datetime': 'GETDATE()', 'date': 'GETDATE()', 'time': "'00:00'", 'uniqueidentifier': 'NEWID()',
            'datetimeoffset': 'SYSDATETIMEOFFSET()'}


def q(s):
    return '[' + s.replace(']', ']]') + ']'


def lit(s):
    return "N'" + s.replace("'", "''") + "'"


def default_for(sqltype):
    base = sqltype.split('(')[0]
    if base in ('nvarchar', 'varchar', 'nchar', 'char'):
        return "N''"
    if base in ('decimal', 'numeric'):
        return '0'
    if base == 'varbinary':
        return '0x'
    return DEFAULTS.get(base, '0')


# ---------------------------------------------------------------- EF snapshot
def parse_snapshot(edmx):
    ssdl = edmx[edmx.find('<StorageModels>'):edmx.find('</StorageModels>')]
    mapping = edmx[edmx.find('<Mappings>'):edmx.find('</Mappings>')]
    etypes = {}
    for m in re.finditer(r'<EntityType Name="([^"]+)"[^>]*>(.*?)</EntityType>', ssdl, re.S):
        body = m.group(2)
        keys = re.findall(r'<PropertyRef Name="([^"]+)"', body.split('</Key>')[0]) if '<Key>' in body else []
        cols, indexes = [], {}
        for p in re.finditer(r'<Property ([^>]*?)/?>', body):
            a = dict(re.findall(r'([\w:]+)="([^"]*)"', p.group(1)))
            t = a['Type']
            if t in ('nvarchar', 'varchar', 'varbinary', 'nchar', 'char', 'binary'):
                t += '(%s)' % a.get('MaxLength', 'max')
            elif t in ('decimal', 'numeric'):
                t += '(%s, %s)' % (a.get('Precision', '18'), a.get('Scale', '2'))
            elif t in ('datetime2', 'time', 'datetimeoffset') and 'Precision' in a:
                t += '(%s)' % a['Precision']
            cols.append({'name': a['Name'], 'type': t, 'null': a.get('Nullable', 'true') == 'true',
                         'identity': a.get('StoreGeneratedPattern') == 'Identity'})
            for ix in re.findall(r'\{ Name: ([^,]+), Order: (\d+)(?:, IsUnique: (True))? \}', a.get('customannotation:Index', '')):
                d = indexes.setdefault(ix[0], {'cols': [], 'unique': bool(ix[2])})
                d['cols'].append((int(ix[1]), a['Name']))
        etypes[m.group(1)] = {'keys': keys, 'cols': cols, 'indexes': indexes}

    sets = {}   # EntitySet name -> table info
    for m in re.finditer(r'<EntitySet Name="([^"]+)" EntityType="[^"]*?\.([^".]+)" Schema="(\w+)" (?:\w+:)?Table="([^"]+)"', ssdl):
        if m.group(4) == '__MigrationHistory':
            continue
        sets[m.group(1)] = dict(etypes[m.group(2)], schema=m.group(3), table=m.group(4), etype=m.group(2))

    # Associations -> foreign keys
    assoc = {}
    for m in re.finditer(r'<Association Name="([^"]+)">(.*?)</Association>', ssdl, re.S):
        b = m.group(2)
        pr = re.search(r'<Principal Role="([^"]+)">(.*?)</Principal>', b, re.S)
        dp = re.search(r'<Dependent Role="([^"]+)">(.*?)</Dependent>', b, re.S)
        if not pr or not dp:
            continue
        cascade = 'OnDelete Action="Cascade"' in b
        assoc[m.group(1)] = (pr.group(1), re.findall(r'Name="([^"]+)"', pr.group(2)),
                             dp.group(1), re.findall(r'Name="([^"]+)"', dp.group(2)), cascade)
    fks = []
    for m in re.finditer(r'<AssociationSet Name="([^"]+)" Association="[^"]*?\.([^".]+)">(.*?)</AssociationSet>', ssdl, re.S):
        if m.group(2) not in assoc:
            continue
        ends = dict(re.findall(r'<End Role="([^"]+)" EntitySet="([^"]+)"', m.group(3)))
        prole, pcols, drole, dcols, cascade = assoc[m.group(2)]
        p, d = sets.get(ends.get(prole)), sets.get(ends.get(drole))
        if p and d:
            fks.append({'ptable': p['table'], 'pschema': p['schema'], 'pcols': pcols,
                        'dtable': d['table'], 'dschema': d['schema'], 'dcols': dcols, 'cascade': cascade})

    entity_to_set = dict(re.findall(r'<EntityTypeMapping TypeName="[^"]*?\.([^".]+)">\s*<MappingFragment StoreEntitySet="([^"]+)"', mapping))
    return sets, fks, entity_to_set


# ---------------------------------------------------------------- C# models
def cs_columns(entity):
    """[(name, sqltype, nullable, is_key)] from the C# class, following base classes in Models/."""
    src, m = chk.find_class(entity)
    if not src:
        return None, []
    table = m.group(2)
    header = src[m.start():m.end()]
    base = re.search(r':\s*([\w.]+)', header.split('class', 1)[1])
    depth, i = 1, m.end()
    while depth and i < len(src):
        depth += {'{': 1, '}': -1}.get(src[i], 0)
        i += 1
    body = src[m.end():i]
    out = []
    if base and base.group(1).split('.')[-1] not in ('Controller', 'DbContext', 'IdentityUser', 'Exception'):
        _, inherited = cs_columns(base.group(1).split('.')[-1])
        out.extend(inherited)
    for pm in re.finditer(r'((?:\[[^\]]*\]\s*)*)public\s+(?!virtual|static|override)([\w\[\]?.<>]+)\s+(\w+)\s*\{\s*get;', body):
        attrs, ptype, pname = pm.groups()
        if 'NotMapped' in attrs:
            continue
        nullable = ptype.endswith('?')
        base_t = ptype.rstrip('?').replace('System.', '')
        if base_t not in CS_TYPES:
            continue
        sqltype = CS_TYPES[base_t]
        if base_t == 'string':
            ln = re.search(r'(?:StringLength|MaxLength)\((\d+)', attrs)
            if ln:
                sqltype = 'nvarchar(%s)' % ln.group(1)
            nullable = 'Required' not in attrs
        elif base_t == 'byte[]':
            nullable = True
        cm = re.search(r'Column\("([^"]+)"', attrs)
        is_key = '[Key' in attrs or 'Key]' in attrs or pname == 'Id'
        out = [c for c in out if c[0] != (cm.group(1) if cm else pname)]
        out.append((cm.group(1) if cm else pname, sqltype, nullable, is_key))
    return table, out


# ---------------------------------------------------------------- SQL emitters
def create_table(schema, table, cols, keys, indexes):
    lines = []
    for c in cols:
        ident = ' IDENTITY(1,1)' if c['identity'] else ''
        lines.append('        %s %s%s %sNULL' % (q(c['name']), c['type'], ident, '' if c['null'] else 'NOT '))
    if keys:
        lines.append('        CONSTRAINT %s PRIMARY KEY (%s)' % (q('PK_%s.%s' % (schema, table)), ', '.join(q(k) for k in keys)))
    s = ["IF OBJECT_ID(%s, 'U') IS NULL" % lit('%s.%s' % (schema, table)), 'BEGIN',
         '    CREATE TABLE %s.%s (' % (q(schema), q(table)), ',\n'.join(lines), '    );']
    for name, ix in sorted(indexes.items()):
        cl = ', '.join(q(c) for _, c in sorted(ix['cols']))
        s.append('    CREATE %sINDEX %s ON %s.%s (%s);' % ('UNIQUE ' if ix['unique'] else '', q(name), q(schema), q(table), cl))
    s += ["    INSERT #created VALUES (%s);" % lit(table), "    PRINT 'Created table %s.%s';" % (schema, table.replace("'", "''")), 'END']
    return '\n'.join(s)


def add_column(schema, table, name, sqltype, nullable, identity=False):
    full = '%s.%s' % (schema, table)
    if identity:
        return None   # cannot add an identity column to an existing table safely
    if nullable:
        add = 'ALTER TABLE %s.%s ADD %s %s NULL;' % (q(schema), q(table), q(name), sqltype)
    else:
        add = 'ALTER TABLE %s.%s ADD %s %s NOT NULL CONSTRAINT %s DEFAULT (%s);' % (
            q(schema), q(table), q(name), sqltype, q('DF_%s_%s' % (table, name)), default_for(sqltype))
    return ("IF OBJECT_ID(%s, 'U') IS NOT NULL AND COL_LENGTH(%s, %s) IS NULL\n"
            "BEGIN\n    %s\n    PRINT 'Added column %s.%s';\nEND") % (
        lit(full), lit(full), lit(name), add, table.replace("'", "''"), name)


def add_fk(fk):
    name = 'FK_%s.%s_%s.%s_%s' % (fk['dschema'], fk['dtable'], fk['pschema'], fk['ptable'], '_'.join(fk['dcols']))
    dfull, pfull = '%s.%s' % (fk['dschema'], fk['dtable']), '%s.%s' % (fk['pschema'], fk['ptable'])
    cols_exist = ' AND '.join('COL_LENGTH(%s, %s) IS NOT NULL' % (lit(dfull), lit(c)) for c in fk['dcols'])
    return ("IF EXISTS (SELECT 1 FROM #created WHERE TableName = %s) AND OBJECT_ID(%s, 'U') IS NOT NULL\n"
            "   AND %s AND OBJECT_ID(%s, 'F') IS NULL\n"
            "    ALTER TABLE %s.%s WITH CHECK ADD CONSTRAINT %s FOREIGN KEY (%s) REFERENCES %s.%s (%s)%s;") % (
        lit(fk['dtable']), lit(pfull), cols_exist, lit('%s.%s' % (fk['dschema'], q(name))),
        q(fk['dschema']), q(fk['dtable']), q(name), ', '.join(q(c) for c in fk['dcols']),
        q(fk['pschema']), q(fk['ptable']), ', '.join(q(c) for c in fk['pcols']),
        ' ON DELETE CASCADE' if fk['cascade'] else '')


def main():
    mig, edmx = chk.latest_snapshot()
    sets, fks, entity_to_set = parse_snapshot(edmx)
    snapshot_tables = {s['table'] for s in sets.values()}

    creates, columns, extra_tables, drift = [], [], [], []
    for s in sorted(sets.values(), key=lambda s: s['table'].lower()):
        creates.append(create_table(s['schema'], s['table'], s['cols'], s['keys'], s['indexes']))
        for c in s['cols']:
            stmt = add_column(s['schema'], s['table'], c['name'], c['type'], c['null'], c['identity'])
            if stmt:
                columns.append(stmt)

    # Columns the C# models have but the snapshot doesn't (added by hand SQL).
    for entity, eset in sorted(entity_to_set.items()):
        s = sets.get(eset)
        if not s:
            continue
        have = {c['name'] for c in s['cols']}
        _, cols = cs_columns(entity)
        for name, sqltype, nullable, _ in cols:
            if name not in have:
                drift.append('%s.%s' % (s['table'], name))
                columns.append(add_column(s['schema'], s['table'], name, sqltype, True))   # always NULL: safe on live data

    # Tables with a DbSet but not in the snapshot (RE_*, AuditTrail_*).
    ctx = open(os.path.join(chk.MVC, 'DataAccessLayer', 'eServicesDbContext.cs'), encoding='utf-8-sig').read()
    ctx = re.sub(r'/\*.*?\*/', '', ctx, flags=re.S)
    ctx = re.sub(r'//[^\n]*', '', ctx)
    for etype, prop in re.findall(r'public\s+(?:virtual\s+)?(?:I?Db)Set<([\w.]+)>\s+(\w+)', ctx):
        etype = etype.split('.')[-1]
        if etype in entity_to_set:
            continue
        table, cols = cs_columns(etype)
        table = table or prop
        if table in snapshot_tables or table in extra_tables or not cols:
            continue
        extra_tables.append(table)
        keys = [n for n, _, _, k in cols if k][:1] or [cols[0][0]]
        cdefs = [{'name': n, 'type': t, 'null': (nl and n not in keys), 'identity': (n in keys and t in ('int', 'bigint'))}
                 for n, t, nl, _ in cols]
        creates.append(create_table('dbo', table, cdefs, keys, {}))
        for c in cdefs:
            stmt = add_column('dbo', table, c['name'], c['type'], True, c['identity'])
            if stmt:
                columns.append(stmt)

    body = '\n\n'.join(creates) + '\nGO\n\n' + '\n\n'.join(columns) + '\nGO\n\n' + '\n\n'.join(add_fk(f) for f in fks)
    sql = f"""/*
  10-Upgrade-Schema.sql  (GENERATED by tools/generate_schema_upgrade.py, do not hand-edit)

  Brings a PLM main database up to the schema the code expects. Additive only:
  creates missing tables (with keys, indexes, and foreign keys for the tables it
  creates) and adds missing columns. Never drops or changes existing objects, so
  it is safe to run on any copy, and safe to run twice.

    sqlcmd -S <server> -E -b -i 10-Upgrade-Schema.sql -v DbName="PropertyLeaseManagementRealEstateFinal"

  New NOT NULL columns on existing tables get a default (0, '' or GETDATE()).
  Columns found only in the C# models are added as NULL.

  EF snapshot              : {mig}
  Tables from C# models    : {', '.join(extra_tables) or 'none'}
  Columns from C# models   : {', '.join(drift) or 'none'}

  Take a backup first:  BACKUP DATABASE [$(DbName)] TO DISK = N'...\\before_upgrade.bak' WITH COPY_ONLY;
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
USE [$(DbName)];
GO
IF OBJECT_ID('tempdb..#created') IS NOT NULL DROP TABLE #created;
CREATE TABLE #created (TableName sysname PRIMARY KEY);
GO

{body}
GO

SELECT TableName AS CreatedTables FROM #created ORDER BY 1;
DROP TABLE #created;
PRINT 'Schema upgrade finished.';
"""
    open(OUT, 'w', encoding='utf-8', newline='\r\n').write(sql)
    print(f'{OUT}: {len(sets)} snapshot tables, {len(fks)} FKs, extra tables {extra_tables}, drift {len(drift)} cols')


if __name__ == '__main__':
    main()
