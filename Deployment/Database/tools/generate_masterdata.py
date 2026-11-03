#!/usr/bin/env python3
"""
Generates ../20-Upgrade-MasterData.sql from a REFERENCE database.

The repo holds ~25 overlapping seed scripts written against different databases
(hard-coded ids, wrong column names, USE statements for other DBs). Running them
in order against a scratch DB, fixing them up as needed (see reference_db/README.md), gives one
reference DB holding the union of all master data. This script reads that DB and
writes ONE clean, idempotent script:

  * rows are matched on their natural key ([Key], Name for roles, SlideNumber, ...)
  * foreign keys are written as key lookups, never as raw ids, so it works on any copy
  * insert-only: existing rows are never changed (AppSettings values stay untouched)

Usage:
  python3 generate_masterdata.py "sqlcmd -S host -U sa -P *** -d ReferenceDb"
"""
import json, os, shlex, subprocess, sys

OUT = os.path.join(os.path.dirname(__file__), '..', '20-Upgrade-MasterData.sql')

# table -> natural key columns. Order matters: parents before children.
TABLES = [
    ('StatusTypes', ['Key']),
    ('Status', ['Key']),
    ('ReferenceTypes', ['Key']),
    ('LocationTypes', ['Key']),
    ('ApplicantTypes', ['Key']),
    ('RCSActionTypes', ['Key']),
    ('ResponsibilityTypes', ['Key']),
    ('ActivityTrackerMessages', ['Key']),
    ('EmailContentTypes', ['Key']),
    ('DocumentTypes', ['Key']),
    ('DocumentCheckLists', ['DocumentTypeId', 'ReferenceTypeId', 'ApplicationId']),
    ('ComplaintCategories', ['Key']),
    ('ComplaintTypes', ['Key']),
    ('PaymentTransgressionCategories', ['Key']),
    ('PaymentTransgressionSeverities', ['Key']),
    ('PaymentTransgressionTypes', ['Key']),
    ('ServiceRequestCategories', ['Key']),
    ('ServiceRequestPriorities', ['Key']),
    ('AspNetRoles', ['Name']),
    ('TrainingSlides', ['SlideNumber']),
    ('ExaminationQuestions', ['QuestionOrder']),
    ('AppSettings', ['Key']),
]
AUDIT_NULL = {'CreatedBySystemUserId', 'ModifiedBySystemUserId'}
NOW = {'CreatedDateTime', 'ModifiedDateTime', 'CapturedDateTime'}


def run(sqlcmd, query):
    cmd = shlex.split(sqlcmd) + ['-y', '0', '-Q', 'SET NOCOUNT ON; ' + query]
    out = subprocess.run(cmd, capture_output=True, text=True, check=True).stdout
    return out.strip()


def rows_json(sqlcmd, query):
    out = run(sqlcmd, query + ' FOR JSON PATH, INCLUDE_NULL_VALUES')
    return json.loads(''.join(out.splitlines()) or '[]')


def lit(v, sqltype):
    if v is None:
        return 'NULL'
    if sqltype == 'bit':
        return '1' if v in (True, 1, '1') else '0'
    if sqltype in ('int', 'bigint', 'smallint', 'tinyint', 'decimal', 'numeric', 'float', 'real', 'money'):
        return str(v)
    return "N'" + str(v).replace("'", "''") + "'"


def q(c):
    return '[' + c + ']'


def main(sqlcmd):
    meta = rows_json(sqlcmd, """
      SELECT t.name AS tbl, c.name AS col, TYPE_NAME(c.user_type_id) AS typ, c.is_nullable AS nul, c.is_identity AS ident,
             rt.name AS ref_tbl, rc.name AS ref_col,
             CASE WHEN COL_LENGTH(QUOTENAME(rt.name), 'Key') IS NOT NULL THEN 1 ELSE 0 END AS ref_has_key
      FROM sys.tables t JOIN sys.columns c ON c.object_id = t.object_id
      LEFT JOIN sys.foreign_key_columns fk ON fk.parent_object_id = c.object_id AND fk.parent_column_id = c.column_id
      LEFT JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
      LEFT JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fk.referenced_column_id
      WHERE t.name IN (%s)""" % ','.join("'%s'" % t for t, _ in TABLES))
    cols = {}
    for m in meta:
        # Older databases lack some EF foreign keys; DepartmentId always points at ApplicationEntities.
        if m['col'] == 'DepartmentId' and not m['ref_tbl']:
            m.update(ref_tbl='ApplicationEntities', ref_col='Id', ref_has_key=1)
        cols.setdefault(m['tbl'], []).append(m)

    out, counts = [], []
    for table, natkey in TABLES:
        if table not in cols:
            continue
        tc = [c for c in cols[table] if not c['ident']]
        fkmap = {c['col']: c for c in tc if c['ref_tbl']}
        # Select each FK as the referenced row's natural key.
        sel = []
        for c in tc:
            if c['col'] in fkmap and fkmap[c['col']]['ref_has_key'] and fkmap[c['col']]['ref_tbl'] != table:
                f = fkmap[c['col']]
                sel.append("(SELECT r.[Key] FROM dbo.%s r WHERE r.%s = x.%s) AS %s" % (q(f['ref_tbl']), q(f['ref_col']), q(c['col']), q(c['col'])))
            else:
                sel.append('x.' + q(c['col']))
        data = rows_json(sqlcmd, 'SELECT %s FROM dbo.%s x ORDER BY %s' % (', '.join(sel), q(table), ', '.join('x.' + q(k) for k in natkey)))
        if not data:
            continue
        typ = {c['col']: c['typ'] for c in tc}

        def expr(c, v):
            if c in AUDIT_NULL:
                return 'NULL'
            if c in NOW and v is not None:
                return 'GETDATE()'
            f = fkmap.get(c)
            if f and f['ref_has_key'] and f['ref_tbl'] != table:
                if v is None:
                    return 'NULL'
                return '(SELECT TOP (1) Id FROM dbo.%s WHERE [Key] = %s)' % (q(f['ref_tbl']), lit(v, 'nvarchar'))
            if f:   # FK to a table without a natural key (departments, users): keep the id only if it exists there
                if v is None:
                    return 'NULL'
                return '(SELECT Id FROM dbo.%s WHERE Id = %s)' % (q(f['ref_tbl']), lit(v, 'int'))
            return lit(v, typ[c])

        out.append('\n-- %s (%d rows, matched on %s)\nPRINT \'%s\';' % (table, len(data), ', '.join(natkey), table))
        n = 0
        for r in data:
            if any(r.get(k) is None for k in natkey):
                continue
            where = ' AND '.join('t.%s = %s' % (q(k), expr(k, r[k])) for k in natkey)
            names = [c['col'] for c in tc]
            vals = [expr(c, r.get(c)) for c in names]
            # Skip rows whose required parent is missing in the target (insert would fail).
            guards = ['%s IS NOT NULL' % expr(c, r[c]) for c in names
                      if c in fkmap and r.get(c) is not None and not next(x for x in tc if x['col'] == c)['nul']]
            cond = 'NOT EXISTS (SELECT 1 FROM dbo.%s t WHERE %s)' % (q(table), where)
            if guards:
                cond += '\n   AND ' + '\n   AND '.join(guards)
            out.append('IF %s\nBEGIN\n    INSERT dbo.%s (%s)\n    VALUES (%s);\n    SET @added += 1;\nEND' % (
                cond, q(table), ', '.join(q(c) for c in names), ', '.join(vals)))
            n += 1
        out.append("PRINT '  ' + CAST(@added AS varchar(10)) + ' added'; SET @total += @added; SET @added = 0;")
        counts.append('%s %d' % (table, n))

    sql = """/*
  20-Upgrade-MasterData.sql  (GENERATED by tools/generate_masterdata.py, do not hand-edit)

  Adds every master-data row the PLM/EHC workflows need and the database lacks:
  %s.

  Insert-only and idempotent: rows are matched on their natural key, existing rows
  are never changed, foreign keys are resolved by key, so it is safe on any copy
  and safe to run twice. Run 10-Upgrade-Schema.sql first.

    sqlcmd -S <server> -E -b -i 20-Upgrade-MasterData.sql -v DbName="PropertyLeaseManagementRealEstateFinal"

  AppSettings rows are only added when missing; check the new ones with
  03-Review-Environment-Settings.sql (they carry dev values such as URLs and user ids).
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
USE [$(DbName)];
GO
BEGIN TRANSACTION;
DECLARE @added int = 0, @total int = 0;
%s

PRINT 'Master data: ' + CAST(@total AS varchar(10)) + ' rows added in total.';
COMMIT TRANSACTION;
""" % ('; '.join(counts), '\n'.join(out))
    with open(OUT, 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(sql)
    print('wrote', os.path.abspath(OUT), '|', '; '.join(counts))


if __name__ == '__main__':
    if len(sys.argv) != 2:
        sys.exit(__doc__)
    main(sys.argv[1])
