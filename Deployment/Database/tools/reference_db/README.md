# Rebuilding 20-Upgrade-MasterData.sql

1. Restore any older PLM backup to a scratch SQL Server (e.g. `BaseAppDB20200114.bak` in the repo root)
   and run `10-Upgrade-Schema.sql` on it.
2. Pre-seed what a dev database always has but no script creates:
   `ApplicationEntities` row Id 1, and `Applications` row `a_rates_clearance_system`.
3. For each file in `seed_scripts_in_order.txt`, run
   `python3 prep_seed_script.py <file> > fixed.sql` and execute `fixed.sql` on the scratch DB.
   The prep step removes `USE` lines and fixes the known script bugs (see the code).
   `All_Status_Metadata.sql` reports ~16 NULL StatusTypeId errors on the way; later scripts add those keys.
4. `python3 ../generate_masterdata.py "sqlcmd -S <scratch> -U sa -P <pwd> -d <scratchdb>"`

Scripts deliberately left out: `PROD_MASTER_SEED.sql` (broken TRY/CATCH, duplicate of the
MasterData/*.sql content), `seed_complaint_lookup_data.sql` (older complaint key scheme), and every
script that creates users, test applications or Real Estate (DPRE) data.
