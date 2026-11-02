# Workout app

Log gym sessions. ASP.NET Core API (`src/Workout.Api`) serves the React build (`web/`) from one container on Azure Container Apps.

## Fast checks
Run after any change, before saying it works:
- API: `dotnet test`
- Web: `cd web && npm run lint && npm run typecheck`

## Run locally
- API: `dotnet run --project src/Workout.Api` (http://localhost:5029)
- Web: `cd web && npm run dev` (proxies `/api` to the API)
- Container: `docker build -t workout-app . && docker run --rm -p 8080:8080 workout-app`
- Deploy: run the manual `Deploy` GitHub workflow; `infra/main.bicep` defines the Azure resources.

## Migrations
`dotnet ef migrations add <Name> --project src/Workout.Api`. They apply at startup.

## Conventions
- Plan is separate from actual: `LoggedSet` records what happened and never lives on a plan.
- `LoggedSet.SetType` comes from the exercise. Weighted sets have weight and reps; timed sets have duration. The API and a DB check constraint both enforce this.
- API tests run the real app and migrations against a temp SQLite file. Do not mock the database.
- Weights are in kg. `web/src/api.ts` holds the API types by hand; keep it in step with `Endpoints/Contracts.cs`.

## Known limits
- SQLite on container storage: live data is lost on scale-to-zero or redeploy. Demo only until a real database is chosen.
- No auth yet, so the live URL is open to anyone.
