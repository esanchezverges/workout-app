# Workout app

Log gym sessions. ASP.NET Core API (`src/Workout.Api`) serves the React build (`web/`) from one container on Azure Container Apps.

## Fast checks
Run after any change, before saying it works:
- API: `dotnet test` (needs Docker; starts SQL Server with Testcontainers)
- Web: `cd web && npm run lint && npm run typecheck`

## Run locally
- Everything: `docker compose up --build` (http://localhost:8080). Data stays in a volume; `docker compose down -v` clears it.
- API alone: `docker compose up -d db`, `dotnet run --project src/Workout.Api -- --migrate`, then `dotnet run --project src/Workout.Api` (http://localhost:5029)
- Web: `cd web && npm run dev` (proxies `/api` to the API)
- Deploy: run the manual `Deploy` GitHub workflow; `infra/main.bicep` defines the Azure resources.

## Migrations
`dotnet ef migrations add <Name> --project src/Workout.Api`. They do not run at startup: apply them with `--migrate`. Compose has a `migrate` service, and on Azure the `workout-migrate` job runs them on deploy.

## Conventions
- Plan is separate from actual: `LoggedSet` records what happened and never lives on a plan.
- `LoggedSet.SetType` comes from the exercise. Weighted sets have weight and reps; timed sets have duration. The API and a DB check constraint both enforce this.
- API tests run the real app and migrations against SQL Server. Do not mock the database.
- Nothing may touch the database except real API calls, including startup and `/healthz`. Each wake of the paused Azure SQL database spends the free allowance.
- Weights are in kg. `web/src/api.ts` holds the API types by hand; keep it in step with `Endpoints/Contracts.cs`.

## Known limits
- Azure SQL free offer: if the monthly allowance runs out, the API fails until the 1st while `/healthz` stays green.
- No auth yet, so the live URL is open to anyone.
