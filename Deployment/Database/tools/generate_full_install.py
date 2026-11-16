#!/usr/bin/env python3
"""
Generates ../00-PLM-Full-Install.sql: ONE script that builds a complete, working PLM database
(schema + all master data + EHC test users) inside an EMPTY database.

Source = a reference database that is known to work end to end (for EHC: the factory-verified
PLM_VerifyDeploy_20261112, trimmed to master data by trim_reference_db.sql).

Unlike 20-Upgrade-MasterData.sql (which upgrades an existing DB and resolves foreign keys by [Key]),
the target here is empty, so rows are inserted with their ORIGINAL ids (IDENTITY_INSERT). Every
foreign key, AppSettings queue owner id and PreferredComplexAreas.LettingOfficerId therefore lines
up exactly as it did in the database the factory tests passed on.

Order inside the script: tables + PK/unique/defaults -> data -> indexes/check constraints ->
foreign keys (WITH CHECK, so the script proves the data is consistent) -> views/functions/procs/triggers.

Usage:
  python3 generate_full_install.py --server host --user sa --password *** --database Ref [--out file]
  (needs: pip install pymssql)
"""
import argparse, datetime, decimal, json, os, sys, uuid


ODBC = False
BATCH = 200  # rows per INSERT statement (SQL Server allows 1000; smaller keeps the script readable in SSMS)


def q(name):
    return '[' + name.replace(']', ']]') + ']'


def lit(v):
    if v is None:
        return 'NULL'
    if isinstance(v, bool):
        return '1' if v else '0'
    if isinstance(v, (int,)):
        return str(v)
    if isinstance(v, float):
        return repr(v)
    if isinstance(v, decimal.Decimal):
        return str(v)
    if isinstance(v, (bytes, bytearray)):
        return '0x' + v.hex() if v else '0x'
    if isinstance(v, datetime.datetime):
        return "'" + v.strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3] + "'"
    if isinstance(v, datetime.date):
        return "'" + v.strftime('%Y-%m-%d') + "'"
    if isinstance(v, datetime.time):
        return "'" + v.strftime('%H:%M:%S.%f') + "'"
    if isinstance(v, uuid.UUID):
        return "'" + str(v) + "'"
    s = str(v).replace("'", "''")
    if '\r' in s or '\n' in s:
        # keep line breaks out of the script text, so a data line reading "GO" can never split a batch
        parts = s.replace('\r\n', '\x00').split('\x00')
        parts = [p_.replace('\r', "' + NCHAR(13) + N'").replace('\n', "' + NCHAR(10) + N'") for p_ in parts]
        return "(N'" + "' + NCHAR(13) + NCHAR(10) + N'".join(parts) + "')"
    return "N'" + s + "'"


def type_sql(t, max_len, prec, scale):
    if t in ('nvarchar', 'nchar'):
        return f'{t}(max)' if max_len == -1 else f'{t}({max_len // 2})'
    if t in ('varchar', 'char', 'varbinary', 'binary'):
        return f'{t}(max)' if max_len == -1 else f'{t}({max_len})'
    if t in ('decimal', 'numeric'):
        return f'{t}({prec},{scale})'
    if t in ('datetime2', 'time', 'datetimeoffset'):
        return f'{t}({scale})'
    if t == 'float':
        return 'float' if prec == 53 else f'float({prec})'
    return t


class Gen:
    def __init__(self, conn):
        self.c = conn

    def rows(self, sql, *args):
        cur = self.c.cursor()
        if ODBC:
            cur.execute(sql.replace('%s', '?'), *args)
        else:
            cur.execute(sql, args or None)
        names = [d[0] for d in cur.description]
        return [dict(zip(names, r)) for r in cur.fetchall()]

    def tables(self):
        return self.rows("""SELECT t.object_id, s.name sch, t.name tbl FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
                            WHERE t.is_ms_shipped=0 ORDER BY s.name, t.name""")

    def columns(self, oid):
        return self.rows("""SELECT c.name, TYPE_NAME(c.user_type_id) typ, c.max_length, c.precision, c.scale, c.is_nullable,
                                   c.is_identity, CAST(ic.seed_value AS bigint) seed, CAST(ic.increment_value AS bigint) incr,
                                   cc.definition computed, cc.is_persisted, dc.name def_name, dc.definition def_def,
                                   c.collation_name
                            FROM sys.columns c
                            LEFT JOIN sys.identity_columns ic ON ic.object_id=c.object_id AND ic.column_id=c.column_id
                            LEFT JOIN sys.computed_columns cc ON cc.object_id=c.object_id AND cc.column_id=c.column_id
                            LEFT JOIN sys.default_constraints dc ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                            WHERE c.object_id=%s ORDER BY c.column_id""", oid)

    def index_cols(self, oid, iid):
        return self.rows("""SELECT c.name, ic.is_descending_key, ic.is_included_column FROM sys.index_columns ic
                            JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
                            WHERE ic.object_id=%s AND ic.index_id=%s ORDER BY ic.key_ordinal, ic.index_column_id""", oid, iid)

    def indexes(self, oid):
        return self.rows("""SELECT index_id, name, type_desc, is_unique, is_primary_key, is_unique_constraint, filter_definition
                            FROM sys.indexes WHERE object_id=%s AND type>0 AND is_hypothetical=0 ORDER BY index_id""", oid)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--odbc', help='pyodbc connection string instead of --server/--user/--password '
                                   '(Windows: "Driver={ODBC Driver 17 for SQL Server};Server=localhost;Database=X;Trusted_Connection=yes")')
    ap.add_argument('--server')
    ap.add_argument('--port', default='1433')
    ap.add_argument('--user')
    ap.add_argument('--password')
    ap.add_argument('--database')
    ap.add_argument('--out', default=os.path.join(os.path.dirname(__file__), '..', '00-PLM-Full-Install.sql'))
    ap.add_argument('--title', default='PLM full install')
    ap.add_argument('--rules', help='JSON: {"exclude": [tables with no data], "filter": {table: "WHERE ..."}}')
    ap.add_argument('--patches', help='SQL file appended after the data (environment fixes, new keys)')
    a = ap.parse_args()
    rules = json.load(open(a.rules)) if a.rules else {}
    exclude = {x.lower() for x in rules.get('exclude', [])}
    filters = {k.lower(): v for k, v in rules.get('filter', {}).items()}
    overrides = {k.lower(): v for k, v in rules.get('override', {}).items()}  # table -> {column: SQL expression}

    global ODBC
    if a.odbc:
        import pyodbc
        ODBC = True
        conn = pyodbc.connect(a.odbc)
        a.database = a.database or conn.getinfo(pyodbc.SQL_DATABASE_NAME)
    else:
        import pymssql
        conn = pymssql.connect(server=a.server, port=a.port, user=a.user, password=a.password, database=a.database,
                               charset='UTF-8', tds_version='7.4')
    g = Gen(conn)
    out = open(a.out, 'w', encoding='utf-8-sig', newline='')
    w = out.write

    tabs = g.tables()
    w(f"""/* =====================================================================================
   {a.title}
   Generated {datetime.datetime.now():%Y-%m-%d %H:%M} by Deployment/Database/tools/generate_full_install.py
   Source database: {a.database}

   Builds the COMPLETE database: {len(tabs)} tables, all master data, EHC test users and
   workflow queue owners. Run it ONCE, inside a NEW EMPTY database:

       CREATE DATABASE PropertyLeaseManagement;   -- any name
       -- then, in SSMS, select that database and execute this file (F5),
       -- or: sqlcmd -S SERVER -E -d PropertyLeaseManagement -b -i 00-PLM-Full-Install.sql

   It refuses to run against a database that already has tables.
   Afterwards run 03-Review-Environment-Settings.sql (AppSettings paths/URLs) and
   02-Create-AppLogin.sql (app login). All test accounts use password Arsenal5@.
   ===================================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
IF DB_NAME() IN ('master','model','msdb','tempdb')
    RAISERROR('Select the new, empty PLM database before running this script.', 20, 1) WITH LOG;
IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
    RAISERROR('This database already has tables. The full install script only runs on an EMPTY database; use PLM-Upgrade-ALL.sql to upgrade an existing one.', 20, 1) WITH LOG;
GO
PRINT 'Step 1/5: tables';
GO
""")
    # schemas
    for r in g.rows("SELECT name FROM sys.schemas WHERE schema_id BETWEEN 5 AND 16383 AND name NOT IN ('guest','INFORMATION_SCHEMA','sys')"):
        w(f"IF SCHEMA_ID('{r['name']}') IS NULL EXEC('CREATE SCHEMA {q(r['name'])}');\nGO\n")

    late = []   # indexes, check constraints
    data = {}   # table -> {'full', 'cols', 'rows'}
    fks = []
    for t in tabs:
        oid, full = t['object_id'], f"{q(t['sch'])}.{q(t['tbl'])}"
        cols = g.columns(oid)
        lines = []
        for c in cols:
            if c['computed']:
                lines.append(f"    {q(c['name'])} AS {c['computed']}{' PERSISTED' if c['is_persisted'] else ''}")
                continue
            s = f"    {q(c['name'])} {type_sql(c['typ'], c['max_length'], c['precision'], c['scale'])}"
            if c['is_identity']:
                s += f" IDENTITY({c['seed']},{c['incr']})"
            s += ' NULL' if c['is_nullable'] else ' NOT NULL'
            if c['def_def'] is not None:
                s += f" CONSTRAINT {q(c['def_name'])} DEFAULT {c['def_def']}"
            lines.append(s)
        for ix in g.indexes(oid):
            ic = g.index_cols(oid, ix['index_id'])
            keys = ', '.join(q(x['name']) + (' DESC' if x['is_descending_key'] else '') for x in ic if not x['is_included_column'])
            incl = [q(x['name']) for x in ic if x['is_included_column']]
            clus = 'CLUSTERED' if ix['type_desc'] == 'CLUSTERED' else 'NONCLUSTERED'
            if ix['is_primary_key']:
                lines.append(f"    CONSTRAINT {q(ix['name'])} PRIMARY KEY {clus} ({keys})")
            elif ix['is_unique_constraint']:
                lines.append(f"    CONSTRAINT {q(ix['name'])} UNIQUE {clus} ({keys})")
            elif ix['type_desc'] in ('CLUSTERED', 'NONCLUSTERED'):
                s = f"CREATE {'UNIQUE ' if ix['is_unique'] else ''}{clus} INDEX {q(ix['name'])} ON {full} ({keys})"
                if incl:
                    s += f" INCLUDE ({', '.join(incl)})"
                if ix['filter_definition']:
                    s += f" WHERE {ix['filter_definition']}"
                late.append(s + ';')
        w(f"CREATE TABLE {full} (\n" + ',\n'.join(lines) + "\n);\nGO\n")
        for ck in g.rows("SELECT name, definition, is_not_trusted FROM sys.check_constraints WHERE parent_object_id=%s", oid):
            late.append(f"ALTER TABLE {full} WITH CHECK ADD CONSTRAINT {q(ck['name'])} CHECK {ck['definition']};")

        # data (collected first; written after the foreign-key consistency pass below)
        datacols = [c for c in cols if not c['computed'] and c['typ'] not in ('timestamp', 'rowversion')]
        if not datacols or t['tbl'].lower() in exclude:
            continue
        cur = conn.cursor()
        collist = ', '.join(q(c['name']) for c in datacols)
        order = ', '.join(q(c['name']) for c in datacols if c['is_identity']) or '1'
        ov = overrides.get(t['tbl'].lower(), {})
        sel = ', '.join(f"{ov[c['name']]} AS {q(c['name'])}" if c['name'] in ov else q(c['name']) for c in datacols)
        cur.execute(f"SELECT {sel} FROM {full} {filters.get(t['tbl'].lower(), '')} ORDER BY {order}")
        rows = [list(r) for r in cur.fetchall()]
        if rows:
            data[t['tbl']] = {'full': full, 'cols': datacols, 'rows': rows}

    fk_list = g.rows("""SELECT fk.name, OBJECT_NAME(fk.parent_object_id) pt, OBJECT_NAME(fk.referenced_object_id) rt,
                                 pc.name pcol, rc.name rcol, pc.is_nullable nul
                          FROM sys.foreign_keys fk
                          JOIN sys.foreign_key_columns k ON k.constraint_object_id=fk.object_id
                          JOIN sys.columns pc ON pc.object_id=k.parent_object_id AND pc.column_id=k.parent_column_id
                          JOIN sys.columns rc ON rc.object_id=k.referenced_object_id AND rc.column_id=k.referenced_column_id
                          WHERE fk.is_disabled=0 AND (SELECT COUNT(*) FROM sys.foreign_key_columns x WHERE x.constraint_object_id=fk.object_id)=1""")
    # Foreign-key consistency: rows were filtered (users, transactions), so a kept row may point at a row that
    # is not kept. A nullable reference is set to NULL; a row with a required reference is dropped. Repeat until stable.
    report = {}
    changed = True
    while changed:
        changed = False
        for fk in fk_list:
            par = data.get(fk['pt'])
            if not par:
                continue
            ref = data.get(fk['rt'])
            ci = [c['name'] for c in par['cols']].index(fk['pcol']) if fk['pcol'] in [c['name'] for c in par['cols']] else None
            if ci is None:
                continue
            if ref and fk['rcol'] in [c['name'] for c in ref['cols']]:
                ri = [c['name'] for c in ref['cols']].index(fk['rcol'])
                keys = {r[ri] for r in ref['rows']}
            else:
                keys = set()
            keep = []
            for r in par['rows']:
                v = r[ci]
                if v is None or v in keys:
                    keep.append(r)
                elif fk['nul']:
                    r[ci] = None
                    keep.append(r)
                    report[(fk['pt'], fk['pcol'], 'set NULL')] = report.get((fk['pt'], fk['pcol'], 'set NULL'), 0) + 1
                    changed = True
                else:
                    report[(fk['pt'], fk['pcol'], 'row dropped')] = report.get((fk['pt'], fk['pcol'], 'row dropped'), 0) + 1
                    changed = True
            par['rows'] = keep
    for (tb, col, act), n in sorted(report.items()):
        print(f'  consistency: {tb}.{col}: {n} {act}', file=sys.stderr)

    w("PRINT 'Step 2/5: master data and test users';\nGO\n")
    total_rows = 0
    for tb, d in data.items():
        rows, datacols, full = d['rows'], d['cols'], d['full']
        if not rows:
            continue
        total_rows += len(rows)
        collist = ', '.join(q(c['name']) for c in datacols)
        ident = any(c['is_identity'] for c in datacols)
        w(f"PRINT '  {tb}: {len(rows)} rows';\n")
        if ident:
            w(f"SET IDENTITY_INSERT {full} ON;\n")
        for i in range(0, len(rows), BATCH):
            chunk = rows[i:i + BATCH]
            w(f"INSERT INTO {full} ({collist}) VALUES\n" +
              ',\n'.join('(' + ', '.join(lit(v) for v in r) + ')' for r in chunk) + ';\n')
        if ident:
            w(f"SET IDENTITY_INSERT {full} OFF;\n")
        w('GO\n')
    if a.patches:
        w("PRINT 'Step 2b: release patches (new keys, environment fixes)';\nGO\n")
        w(open(a.patches, encoding='utf-8-sig').read().strip() + '\nGO\n')

    w("PRINT 'Step 3/5: indexes and check constraints';\nGO\n")
    for s in late:
        w(s + '\nGO\n')

    w("PRINT 'Step 4/5: foreign keys (checked against the data)';\nGO\n")
    for fk in g.rows("""SELECT fk.object_id, fk.name, OBJECT_SCHEMA_NAME(fk.parent_object_id) ps, OBJECT_NAME(fk.parent_object_id) pt,
                               OBJECT_SCHEMA_NAME(fk.referenced_object_id) rs, OBJECT_NAME(fk.referenced_object_id) rt,
                               fk.delete_referential_action_desc del, fk.update_referential_action_desc upd, fk.is_disabled
                        FROM sys.foreign_keys fk ORDER BY fk.name"""):
        cols = g.rows("""SELECT pc.name pcol, rc.name rcol FROM sys.foreign_key_columns k
                         JOIN sys.columns pc ON pc.object_id=k.parent_object_id AND pc.column_id=k.parent_column_id
                         JOIN sys.columns rc ON rc.object_id=k.referenced_object_id AND rc.column_id=k.referenced_column_id
                         WHERE k.constraint_object_id=%s ORDER BY k.constraint_column_id""", fk['object_id'])
        s = (f"ALTER TABLE {q(fk['ps'])}.{q(fk['pt'])} WITH CHECK ADD CONSTRAINT {q(fk['name'])} FOREIGN KEY "
             f"({', '.join(q(c['pcol']) for c in cols)}) REFERENCES {q(fk['rs'])}.{q(fk['rt'])} ({', '.join(q(c['rcol']) for c in cols)})")
        if fk['del'] != 'NO_ACTION':
            s += ' ON DELETE ' + fk['del'].replace('_', ' ')
        if fk['upd'] != 'NO_ACTION':
            s += ' ON UPDATE ' + fk['upd'].replace('_', ' ')
        w(s + ';\n')
        if fk['is_disabled']:
            w(f"ALTER TABLE {q(fk['ps'])}.{q(fk['pt'])} NOCHECK CONSTRAINT {q(fk['name'])};\n")
        fks.append(fk['name'])
    w('GO\n')

    w("PRINT 'Step 5/5: views, functions, procedures, triggers';\nGO\n")
    mods = g.rows("""SELECT o.object_id, o.type, OBJECT_SCHEMA_NAME(o.object_id) sch, o.name, m.definition
                     FROM sys.sql_modules m JOIN sys.objects o ON o.object_id=m.object_id
                     WHERE o.is_ms_shipped=0 AND o.type IN ('V','FN','IF','TF','P','TR')""")
    deps = {}
    for d in g.rows("""SELECT referencing_id, referenced_id FROM sys.sql_expression_dependencies WHERE referenced_id IS NOT NULL"""):
        deps.setdefault(d['referencing_id'], set()).add(d['referenced_id'])
    ids = {m['object_id'] for m in mods}
    done, ordered = set(), []

    def visit(m, stack=()):
        if m['object_id'] in done or m['object_id'] in stack:
            return
        for dep in deps.get(m['object_id'], ()):
            if dep in ids:
                visit(next(x for x in mods if x['object_id'] == dep), stack + (m['object_id'],))
        done.add(m['object_id'])
        ordered.append(m)
    for m in sorted(mods, key=lambda m: ('TR' == m['type'].strip(), m['name'])):
        visit(m)
    for m in ordered:
        w(m['definition'].strip() + '\nGO\n')

    w(f"""PRINT 'Full install finished: {len(tabs)} tables, {total_rows} rows, {len(fks)} foreign keys, {len(ordered)} programmable objects.';
GO
""")
    out.close()
    print(f'wrote {a.out}: {len(tabs)} tables, {total_rows} rows, {len(fks)} FKs, {len(ordered)} modules', file=sys.stderr)


if __name__ == '__main__':
    main()
