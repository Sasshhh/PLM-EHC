insert into statustypes (name, description, [key], isactive, isdeleted, islocked) values ('Masterpass', 'Masterpass Transaction Status', 'st_masterpass', 1, 0, 0);
insert into status (name, description, [key], isactive, isdeleted, islocked, statustypeid) values ('Recieved', 'Masterpass Notification Recieved', 's_masterpass_notication_recieved', 1, 0, 0, (select id from statustypes where [key] like 'st_masterpass'));
insert into status (name, description, [key], isactive, isdeleted, islocked, statustypeid) values ('Processed', 'Masterpass Notification Processed', 's_masterpass_notication_processed', 1, 0, 0, (select id from statustypes where [key] like 'st_masterpass'));
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Url', 'Base Url for Masterpass Integration', 'as_masterpass_url', 'https://masterpasstest.oltio.co.za/pluto', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Username', 'Masterpass Username', 'as_masterpass_username', 'merchant-25233', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Password', 'Masterpass Username', 'as_masterpass_password', 'DCAA32FC375145B85BDB6FE36E9AC550', 1, 0, 0)

insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('SMPP Host', 'SMPP Host', 'as_smpp_host', 'smsgw5.gsm.co.za', 1, 0, 0)

insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('SMPP Username', 'SMPP Username', 'as_smpp_username', 'dyna01', 1, 0, 0)

insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('SMPP Password', 'SMPP Username', 'as_smpp_password', 'dyna01', 1, 0, 0)

insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('SMPP Port', 'SMPP Port', 'as_smpp_port', '2775', 1, 0, 0)

INSERT [dbo].[AppSettings] ([Name], [Description], [Key], [Value], [IsActive], [IsDeleted], [IsLocked], [CreatedBySystemUserId], [CreatedDateTime], [ModifiedBySystemUserId], [ModifiedDateTime]) VALUES (N'Imagin8 AVS Customer ID', N'Imagin8 AVS Customer Username', N'avs_customer_id', N'10190', 1, 0, 0, 1, CAST(N'2019-06-28 23:15:00.000' AS DateTime), NULL, NULL)
GO
INSERT [dbo].[AppSettings] ([Name], [Description], [Key], [Value], [IsActive], [IsDeleted], [IsLocked], [CreatedBySystemUserId], [CreatedDateTime], [ModifiedBySystemUserId], [ModifiedDateTime]) VALUES (N'Imagin8 AVS Customer Key', N'Imagin8 AVS Customer API Key', N'avs_customer_api_key', N'89B4036F-9908-4764-B2F4-847BC5AC8168', 1, 0, 0, 1, CAST(N'2019-06-28 23:17:00.000' AS DateTime), NULL, NULL)
GO
INSERT [dbo].[AppSettings] ([Name], [Description], [Key], [Value], [IsActive], [IsDeleted], [IsLocked], [CreatedBySystemUserId], [CreatedDateTime], [ModifiedBySystemUserId], [ModifiedDateTime]) VALUES (N'Imagin8 AVS Enquiry URL', N'Imagin8 AVS First AVS URL', N'avs_first_check_url', N'https://www.imagin8.co.za/api/json/sandbox/avsr/SubmitFile?', 1, 0, 0, 1, CAST(N'2019-06-28 23:20:00.000' AS DateTime), NULL, NULL)
GO
INSERT [dbo].[AppSettings] ([Name], [Description], [Key], [Value], [IsActive], [IsDeleted], [IsLocked], [CreatedBySystemUserId], [CreatedDateTime], [ModifiedBySystemUserId], [ModifiedDateTime]) VALUES (N'Imagin8 AVS Check URL', N'Imagin8 AVS 14 Day Check URL', N'avs_check_14_day_url', N'https://www.imagin8.co.za/api/json/sandbox/avsr/CheckJobID?JobID=', 1, 0, 0, 1, CAST(N'2019-06-28 23:23:00.000' AS DateTime), NULL, NULL)

insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Daily Url', 'Masterpass Daily Url', 'as_masterpass_daily_url', 'https://masterpasstest.oltio.co.za/portal/restful/transactionsByDate', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Daily Url Path', 'Masterpass Daily Url Path', 'as_masterpass_daily_path_url', 'c:\tmp\', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Daily Start Time', 'Masterpass Daily Start Time', 'as_masterpass_daily_starttime', '00:00:00', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Daily End Time', 'Masterpass Daily End Time', 'as_masterpass_daily_endtime', '00:00:00', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Daily Filename', 'Masterpass Daily Filename', 'as_masterpass_daily_filename', 'DailyRecon', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Daily Execution Time', 'Masterpass Daily Execution Time', 'as_masterpass_daily_executiontime', '00:15:00', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Hourly Path', 'Masterpass Hourly Path', 'as_masterpass_hourly_path', 'c:\tmp\', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Hourly Filename', 'Masterpass Hourly Filename', 'as_masterpass_hourly_filename', 'FileRecon', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Payment History Polling Directory', 'Payment History Polling Directory', 'as_paymenthistory_polling_directory', 'c:\tmp\drop\', 1, 0, 0)


insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('ACB Schedule One', 'ACB Schedule One', 'acb_schedule_one', '10:25', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('ACB Schedule Two', 'ACB Schedule Two', 'acb_schedule_two', '10:30', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('ACB Filename', 'ACB Filename', 'acb_filename', 'nedsIF2', 1, 0, 0)
insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('ACB Path', 'ACB Path', 'acb_path', 'c:\tmp\', 1, 0, 0)

insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Username', 'Masterpass Username', 'as_masterpass_username', 'merchant-9208', 1, 0, 0)

insert into appsettings (name, description, [key], value, isactive, isdeleted, islocked) values ('Masterpass Password', 'Masterpass Password', 'as_masterpass_password', 'E93C8BDA33A46F9109156C90BBB19E99', 1, 0, 0)
insert into appsettings (name, [description], [key], value, isactive, isdeleted, islocked) values ('SMPP Host',	'SMPP Host', 'as_smpp_host',	'smsgw5.gsm.co.za',1,0,0);
insert into appsettings (name, [description], [key], value, isactive, isdeleted, islocked) values ('SMPP Username',	'SMPP Username',	'as_smpp_username',	'dyna01',1,0,0);
insert into appsettings (name, [description], [key], value, isactive, isdeleted, islocked) values ('SMPP Password',	'SMPP Password',	'as_smpp_password',	'dyna01',1,0,0);
insert into appsettings (name, [description], [key], value, isactive, isdeleted, islocked) values ('SMPP Port',	'SMPP Port', 'as_smpp_port', '2775',1,0,0);

insert into appsettings (name, [description], [key], value, isactive, isdeleted, islocked) values ('Masterpass Test',	'Test for Masterpass URL validation', 'as_masterpass_test', 'true',1,0,0);

insert into appsettings (name, [description], [key], value, isactive, isdeleted, islocked) values ('Masterpass Hourly',	'Masterpass hourly interval setting', 'as_masterpass_hourly_interval', '120',1,0,0);

