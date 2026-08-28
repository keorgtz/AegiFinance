#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."
backup_file="${1:-}"
if [[ -z "$backup_file" ]]; then
  backup_file="$(docker compose exec -T backup bash -lc "ls -1t /var/opt/mssql/backup/AegiFinanceDb-*.bak | head -n 1" | tr -d '\r')"
fi
if [[ -z "$backup_file" ]]; then echo "No backup file was found." >&2; exit 1; fi

restore_db="AegiFinanceRestoreVerification"
cleanup() {
  docker compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b \
    -Q "IF DB_ID(N'$restore_db') IS NOT NULL BEGIN ALTER DATABASE [$restore_db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$restore_db]; END" >/dev/null || true
}
trap cleanup EXIT

source ./.env
export MSSQL_SA_PASSWORD
logical_data="$(docker compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -h-1 -W -Q "RESTORE FILELISTONLY FROM DISK=N'$backup_file'" | awk 'NR==1{print $1}' | tr -d '\r')"
logical_log="$(docker compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -h-1 -W -Q "RESTORE FILELISTONLY FROM DISK=N'$backup_file'" | awk '$3=="L"{print $1; exit}' | tr -d '\r')"

docker compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b -Q "
RESTORE VERIFYONLY FROM DISK=N'$backup_file' WITH CHECKSUM;
RESTORE DATABASE [$restore_db] FROM DISK=N'$backup_file' WITH MOVE N'$logical_data' TO N'/var/opt/mssql/data/$restore_db.mdf', MOVE N'$logical_log' TO N'/var/opt/mssql/data/${restore_db}_log.ldf', RECOVERY, REPLACE;
DBCC CHECKDB(N'$restore_db') WITH NO_INFOMSGS, ALL_ERRORMSGS;
SELECT COUNT_BIG(*) AS OrganizationCount FROM [$restore_db].[dbo].[Organizations];"

mkdir -p evidence
printf 'backup=%s\nverified_at_utc=%s\nrpo_hours=6\nrto_hours=2\nresult=passed\n' "$backup_file" "$(date -u +%FT%TZ)" > evidence/phase15-restore.txt
echo "Restore rehearsal passed. Evidence: evidence/phase15-restore.txt"
