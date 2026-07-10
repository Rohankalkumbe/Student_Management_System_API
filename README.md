# Student Management System API

## Run with Docker

Build and run the API locally:

```bash
docker build -t student-management-api .
docker run --rm -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=<your-sql-server-connection-string>" \
  student-management-api
```

Open Swagger at `http://localhost:8080/swagger` and the health check at
`http://localhost:8080/health`.

## Deploy on Render

1. Push this repository, including `Dockerfile` and `render.yaml`, to GitHub.
2. In Render, create a **Blueprint** from the repository (or create a Web
   Service and select the Docker runtime).
3. Set the `ConnectionStrings__DefaultConnection` environment variable in the
   Render dashboard to a connection string for a reachable SQL Server database.
   For Azure SQL, use for example:

   ```text
   Server=tcp=<server>.database.windows.net,1433;Initial Catalog=<database>;User ID=<user>;Password=<password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```

   Ensure the database firewall permits connections from Render. Do not use the
   local `(localdb)` connection string in Render.
4. Deploy. Startup runs `Database.Migrate()`, which creates the `Students`
   table and applies all pending EF Core migrations. A migration failure is
   logged and intentionally fails the deployment rather than allowing requests
   to return a hidden 500 error.

To apply migrations manually from a machine that has access to the production
database, set the same environment variable and run:

```bash
dotnet ef database update --configuration Release
```

LocalDB (`(localdb)\\MSSQLLocalDB`) works only on a local Windows machine, so it
cannot be used from Render. Use a hosted SQL Server provider (for example Azure
SQL) and keep its credentials only in Render's environment settings.
