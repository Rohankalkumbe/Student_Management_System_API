using Microsoft.AspNetCore.Diagnostics;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var renderPort = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(renderPort, out var port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var connectionString = builder.Configuration["MongoDb:ConnectionString"];
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "MongoDb__ConnectionString must be configured. " +
        "Set it to a reachable MongoDB connection string.");
}

var databaseName = builder.Configuration["MongoDb:DatabaseName"] ?? "StudentManagementSystem";
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
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
    var isDatabaseError = exception?.GetBaseException() is MongoException or TimeoutException;
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
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student API V1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
