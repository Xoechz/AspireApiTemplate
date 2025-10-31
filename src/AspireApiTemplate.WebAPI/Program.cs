using System.Diagnostics;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using AspireApiTemplate.Logic.Extensions;
using AspireApiTemplate.Data;
using Microsoft.EntityFrameworkCore;

namespace AspireApiTemplate.WebAPI;

internal class Program
{
    #region Private Methods

    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Enable HTTP/1, HTTP/2, HTTP/3 on all configured endpoints (Aspire injects ports)
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);
        });

        // Add db context
        var connectionString = builder.Configuration.GetConnectionString("DBTEMPLATENAME")
            ?? throw new InvalidOperationException("Connection String DB is not configured.");
        builder.Services.AddDbContext<TemplateContext>(options => options.UseSqlServer(connectionString));

        ConfigureOpenTelemetry(builder.Services, builder.Environment);

        AddServices(builder.Services);

        var app = builder.Build();

        ConfigureMiddleware(app);

        await app.RunAsync();
    }

    /// <summary>
    /// Adds services to the container. Put our own services here as well.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    private static void AddServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen();
        services.AddLogicServices();
    }

    /// <summary>
    /// Configures the middleware pipeline.
    /// </summary>
    /// <param name="app"><see cref="WebApplication"/></param>
    private static void ConfigureMiddleware(WebApplication app)
    {
        app.MapControllers();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    /// <summary>
    /// Configures OpenTelemetry for tracing, metrics and logging. Also injects an ActivitySource for manual trace instrumentation.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="environment"><see cref="IWebHostEnvironment"/></param>
    private static void ConfigureOpenTelemetry(IServiceCollection services, IWebHostEnvironment environment)
    {
        var serviceName = environment.ApplicationName;

        // for manual trace instrumentation
        // to use it, inject the ActivitySource into you class and then create an Activity like this:
        // using (var activity = activitySource.StartActivity("MyOperation"))
        // Do not forget the using statement or dispose the activity manually to end the trace.
        var activitySource = new ActivitySource(serviceName);
        services.AddSingleton(activitySource);

        // OpenTelemetry configuration
        services.AddOpenTelemetry()
            // Add the service name to the resource attributes so application insights displays it correctly in the application map
            .ConfigureResource(rb => rb.AddService(serviceName))
            // Add the logging provider to export logs of the ILogger interface
            .WithLogging()
            // Metrics are used to collect performance data, such as request duration, memory usage, etc.
            .WithMetrics(metrics =>
            {
                // AspNetCoreInstrumentation adds general metrics for ASP.NET Core applications such as request duration, request count, etc.
                metrics.AddAspNetCoreInstrumentation()
                    // HttpClientInstrumentation adds metrics for outgoing HTTP requests made by the application.
                    .AddHttpClientInstrumentation()
                    // SqlClientInstrumentation adds metrics for SQL database operations.
                    .AddSqlClientInstrumentation()
                    // RuntimeInstrumentation adds metrics for the .NET runtime, such as garbage collection, thread pool usage, etc.
                    .AddRuntimeInstrumentation();
            })
            // Tracing is used to collect detailed information about the execution of requests, jobs and other operations in the application.
            .WithTracing(tracing =>
            {
                // Adds the service name, so the logs can be correlated with traces
                tracing.AddSource(serviceName)
                    // AspNetCoreInstrumentation adds tracing for ASP.NET Core operations, such as HTTP requests, middleware, etc.
                    .AddAspNetCoreInstrumentation()
                    // EntityFrameworkCoreInstrumentation adds tracing for Entity Framework Core operations, such as database queries and commands.
                    // Node: The SqlClientInstrumentation is not included, because we do not really use raw SQL commands, but hangfire uses them fairly often, which is not relevant.
                    // Also the EntityFramework Traces would be duplicated.
                    // The DB statements are captured to see which SQL commands are executed.
                    .AddEntityFrameworkCoreInstrumentation(o => o.EnrichWithIDbCommand = (activity, command) =>
                    {
                        activity.SetTag("db.statement", command.CommandText);
                    })
                    // HttpClientInstrumentation adds tracing for outgoing HTTP requests made by the application.
                    .AddHttpClientInstrumentation();
            })
            // Export telemetry using the OTLP protocol. Aspire sets the correct endpoint automatically.
            .UseOtlpExporter();
    }

    #endregion Private Methods
}