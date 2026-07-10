using System.Data.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;

var builder = WebApplication.CreateBuilder(args);

var renderPort = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(renderPort, out var port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings__DefaultConnection must be configured. " +
        "Set it in Render to a reachable SQL Server/Azure SQL connection string.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                "https://student-management-system-project-omega.vercel.app",
                "http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("Frontend");
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var isDatabaseError = exception?.GetBaseException() is DbException or TimeoutException;
    var statusCode = isDatabaseError ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status500InternalServerError;
    var title = isDatabaseError ? "Database unavailable" : "Unexpected server error";
    var detail = isDatabaseError
        ? "The database could not be reached. Please retry shortly."
        : "The request could not be completed. Use the trace ID when contacting support.";

    app.Logger.LogError(exception, "Unhandled request error. TraceId: {TraceId}", context.TraceIdentifier);
    await Results.Problem(
        statusCode: statusCode,
        title: title,
        detail: detail,
        extensions: new Dictionary<string, object?> { ["traceId"] = context.TraceIdentifier })
        .ExecuteAsync(context);
}));
app.UseCors("Frontend");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        app.Logger.LogInformation("Applying pending EF Core migrations.");
        database.Database.Migrate();
        app.Logger.LogInformation("EF Core migrations are up to date.");
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "Database migration failed; the API will not start.");
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student API V1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
