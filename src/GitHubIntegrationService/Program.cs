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
using GitHubIntegrationService.Models;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("Starting GitHub Integration WCF Service...");

// Bind Settings
builder.Services.Configure<GitHubSettings>(builder.Configuration.GetSection("GitHub"));
builder.Services.Configure<QuartzSettings>(builder.Configuration.GetSection("Quartz"));

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
var quartzSettings = builder.Configuration.GetSection("Quartz").Get<QuartzSettings>() ?? new QuartzSettings();
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    
    var jobKey = new JobKey("GitHubCommitJob");
    q.AddJob<GitHubCommitJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("GitHubCommitJob-trigger")
        .WithSimpleSchedule(x => x.WithIntervalInMinutes(quartzSettings.IntervalInMinutes).RepeatForever())); // Read interval from config
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
