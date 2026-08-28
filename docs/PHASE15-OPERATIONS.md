# Phase 15 production runbook

## Recovery objectives

- RPO: 6 hours. The `backup` service creates a native SQL Server backup with `CHECKSUM` every 6 hours and retains 14 days locally.
- RTO: 2 hours from incident declaration to a verified API readiness response.
- Data-protection keys and client documents are included in the host backup policy; a SQL backup alone is not a complete recovery.

## Restore rehearsal

Run on the Ubuntu host after deployment and after every SQL Server or storage change:

```bash
chmod +x scripts/verify-backup-restore.sh
scripts/verify-backup-restore.sh
```

The script restores the latest backup into a disposable database, runs `DBCC CHECKDB`, verifies a core table and removes the database. Its timestamped result is written to `evidence/phase15-restore.txt`. Copy that evidence to durable operational storage.

## Monitoring and alerts

- `/health/live`: process liveness; no database dependency.
- `/health/ready`: API and database readiness; used by Compose.
- `/api/operations/metrics`: protected Prometheus text metrics.
- `/api/operations/outbox`: protected queue health and dead-letter visibility.
- Alert when readiness fails for 2 minutes, worker is unhealthy, any dead-letter exists, or the latest verified backup is older than 7 hours.

## Outbox guarantees

Events have an organization-scoped unique idempotency key, durable retry state, exponential backoff and dead-letter terminal state. External receivers receive the same `Idempotency-Key` on every attempt and must persist it before applying side effects. A missing webhook records the internal event as completed without exporting client data.

## Offline contract

The service worker caches only the application shell and public static assets. It never caches `/api/*`. Financial writes fail immediately while offline and are never replayed automatically. Draft storage exists only through the explicit draft API, which requires both the signed-in user and organization scope.

## Production acceptance

1. `docker compose config --quiet` passes and all five services become healthy/running.
2. Restore rehearsal passes and evidence is archived.
3. Phase 15 E2E passes in Desktop Chrome and Mobile Chrome.
4. No dead letters remain unexplained; alert delivery is exercised.
5. Login, permissions, tenant isolation and logout are confirmed against the public HTTPS domain.
6. Support has the rollback command, current backup location and incident contacts.
