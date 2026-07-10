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
4. Deploy. Render provides `PORT`; the API automatically listens on it.

LocalDB (`(localdb)\\MSSQLLocalDB`) works only on a local Windows machine, so it
cannot be used from Render. Use a hosted SQL Server provider (for example Azure
SQL) and keep its credentials only in Render's environment settings.
