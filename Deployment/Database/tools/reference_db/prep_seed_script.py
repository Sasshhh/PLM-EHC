import sys,re
f=sys.argv[1]
s=open(f,encoding='utf-8-sig',errors='replace').read()
s=re.sub(r'(?im)^\s*USE\s+\[?[\w]+\]?.*$', '-- (USE removed)', s)
s=re.sub(r'(?m)^Msg \d+, Level.*\n.*\n', '', s)
name=f.split('/')[-1]
if name=='sync_masterdata_to_target.sql':
    s=s.replace("ROLLBACK TRANSACTION;\n    PRINT 'ERROR: ' + ERROR_MESSAGE();","ROLLBACK TRANSACTION;\n    THROW;")
    s=re.sub(r"(MERGE INTO dbo\.DocumentCheckLists AS tgt USING \(SELECT\s+(\(SELECT Id FROM dbo\.DocumentTypes\s+WHERE \[Key\] = '[^']+'\) AS DocumentTypeId,\s+\(SELECT Id FROM dbo\.ReferenceTypes\s+WHERE \[Key\] = '([^']+)'\) AS ReferenceTypeId,))",
             lambda m: m.group(1)+" (SELECT Id FROM dbo.Applications WHERE [Key] = '%s') AS ApplicationId," % ('a_rates_clearance_system' if m.group(3)=='rt_upload_rcs' else 'a_all_applications'), s)
    s=s.replace("INSERT (DocumentTypeId,ReferenceTypeId,IsActive,IsDeleted)   VALUES (src.DocumentTypeId,src.ReferenceTypeId,src.IsActive,src.IsDeleted)",
                "INSERT (DocumentTypeId,ReferenceTypeId,ApplicationId,IsActive,IsDeleted)   VALUES (src.DocumentTypeId,src.ReferenceTypeId,src.ApplicationId,src.IsActive,src.IsDeleted)")
if name=='All_ActivityTrackerMessages_Metadata.sql':
    s=re.sub(r"INSERT INTO ActivityTrackerMessages \(\[Key\], \[Description\],(.*?)VALUES \('([^']*)', ('(?:[^']|'')*'),",
             r"INSERT INTO ActivityTrackerMessages ([Key], [Name], [Description],\1VALUES ('\2', \3, \3,", s)
if name=='UC021_RenewalAgreement_StatusInserts.sql':
    s=s.replace('IsLocked, IsNew, CreatedDateTime','IsLocked, CreatedDateTime').replace('0, 0, 0, @Now, @Now','0, 0, @Now, @Now')
if name in ('add_facilities_manager_responsibility_type.sql','MASTER_SETUP_MAINTENANCE_WORKFLOW.sql'):
    s=s.replace('CapturedDateTime','CreatedDateTime')
if name=='MASTER_SETUP_MAINTENANCE_WORKFLOW.sql':
    s=re.sub(r'INSERT INTO dbo\.AppSettings \(\[Key\], \[Value\]', 'INSERT INTO dbo.AppSettings ([Name], [Key], [Value]', s)
    s=re.sub(r"VALUES\s*\(\s*'(u_[a-z_]+)',", lambda m: "VALUES ('%s', '%s'," % (m.group(1), m.group(1)), s)
if name=='seed_complaint_lookup_data.sql':
    s=s.split('-- 3. Verify Seed Data')[0]
print(s)
