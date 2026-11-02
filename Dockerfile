FROM node:24-alpine AS web
WORKDIR /repo/web
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /repo
COPY src/Workout.Api/Workout.Api.csproj src/Workout.Api/
RUN dotnet restore src/Workout.Api/Workout.Api.csproj
COPY src/ src/
COPY --from=web /repo/src/Workout.Api/wwwroot src/Workout.Api/wwwroot
RUN dotnet publish src/Workout.Api/Workout.Api.csproj -c Release --no-restore -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=api /publish .
# Container storage: wiped on scale-to-zero or redeploy. Demo data only.
RUN mkdir /data && chown "$APP_UID" /data
ENV ConnectionStrings__Workout="Data Source=/data/workout.db"
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Workout.Api.dll"]
