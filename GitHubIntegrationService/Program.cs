using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using GitHubIntegrationService.Jobs;
using GitHubIntegrationService.Services;
using Quartz;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Ensure Logs directory exists
var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "Logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(logDirectory, "log-.txt"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("Starting GitHub Integration WCF Service...");

// Add CoreWCF services to the container.
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, ServiceMetadataBehavior>(provider => new ServiceMetadataBehavior { HttpGetEnabled = true });
builder.Services.AddSingleton<IServiceBehavior, ServiceDebugBehavior>(provider => new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

// Add application services
builder.Services.AddHttpClient<IGitHubService, GitHubService>();
builder.Services.AddTransient<GitHubService>();
builder.Services.AddSingleton<CommitCache>();
builder.Services.AddHealthChecks();

// Configure Quartz.NET
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    
    var jobKey = new JobKey("GitHubCommitJob");
    q.AddJob<GitHubCommitJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("GitHubCommitJob-trigger")
        .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever())); // Check every 1 minute for this task
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<GitHubService>();
    serviceBuilder.AddServiceEndpoint<GitHubService, IGitHubService>(new BasicHttpBinding(), "/GitHubService.svc");
});

app.MapHealthChecks("/health");

Log.Information("Service is ready. Endpoints configured.");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Service terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
