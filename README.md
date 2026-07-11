# Student Management System API

This ASP.NET Core API stores students in MongoDB. It uses the
`StudentManagementSystem` database and automatically creates its `students`
and `counters` collections when data is added.

## Configuration

Set `MongoDb__ConnectionString` to your MongoDB connection string. Keep the
connection string out of source control, because it contains credentials.

For local development with .NET user secrets:

```bash
dotnet user-secrets set "MongoDb:ConnectionString" "<your-mongodb-connection-string>"
```

For Render, add `MongoDb__ConnectionString` as a secret environment variable.
The provided `render.yaml` already declares it.

## Run with Docker

```bash
docker build -t student-management-api .
docker run --rm -p 8080:8080 \
  -e "MongoDb__ConnectionString=<your-mongodb-connection-string>" \
  student-management-api
```

Open Swagger at `http://localhost:8080/swagger`.
