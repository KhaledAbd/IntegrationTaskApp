using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using GitHubIntegrationService.Jobs;
using GitHubIntegrationService.Services;
using Quartz;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add CoreWCF services to the container.
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, ServiceMetadataBehavior>(provider => new ServiceMetadataBehavior { HttpGetEnabled = true });

// Add application services
builder.Services.AddHttpClient<IGitHubService, GitHubService>();
builder.Services.AddSingleton<CommitCache>();

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

app.Run();
