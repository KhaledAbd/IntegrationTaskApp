using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using GitHubIntegrationService.Models;
using GitHubIntegrationService.Services;
using System.Linq;

namespace GitHubIntegrationService.Jobs
{
    public class GitHubCommitJob : IJob
    {
        private readonly ILogger<GitHubCommitJob> _logger;
        private readonly HttpClient _httpClient;
        private readonly CommitCache _cache;
        private readonly string _token;
        private readonly string _repoUrl;
        private readonly string _userAgent;

        public GitHubCommitJob(ILogger<GitHubCommitJob> logger, HttpClient httpClient, CommitCache cache, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            _token = configuration["GitHub:Token"] ?? string.Empty;
            _repoUrl = configuration["GitHub:RepoUrl"] ?? string.Empty;
            _userAgent = configuration["GitHub:UserAgent"] ?? "IntegrationTaskApp";
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Scheduled Job 'GitHubCommitJob' started at {Time}", DateTime.UtcNow);

            try
            {
                _logger.LogInformation("Job Refreshing GitHub commits cache for repository: {RepoUrl}", _repoUrl);

                var lastSyncTime = _cache.GetLastSyncTime();
                var existingCommits = _cache.GetCommits();

                var url = _repoUrl.Contains("?") ? $"{_repoUrl}&per_page=100" : $"{_repoUrl}?per_page=100";
                
                if (lastSyncTime > DateTime.MinValue)
                {
                    url += $"&since={lastSyncTime.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}";
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                request.Headers.UserAgent.ParseAdd(_userAgent);

                _logger.LogInformation("Job Sending request to GitHub API...");
                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Job GitHub API request failed with Status Code: {StatusCode}", response.StatusCode);
                    return;
                }

                _logger.LogInformation("Job GitHub API request was successful.");
                var content = await response.Content.ReadAsStringAsync();
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var commits = JsonSerializer.Deserialize<List<GitHubCommit>>(content, options);

                if (commits != null && commits.Any())
                {
                    var newCommits = commits.Where(c => !existingCommits.Any(ec => ec.Sha == c.Sha)).ToList();
                    
                    if (newCommits.Any())
                    {
                        var allCommits = newCommits.Concat(existingCommits)
                            .OrderByDescending(c => c.Commit?.Author?.Date)
                            .ToList();
                            
                        _logger.LogInformation("Job Updating local cache with {Count} new commits (Total: {Total}).", newCommits.Count, allCommits.Count);
                        _cache.UpdateCommits(allCommits);
                    }
                    else
                    {
                        _logger.LogInformation("Job found no new commits after deduplication. Updating sync time.");
                        _cache.UpdateCommits(existingCommits);
                    }
                }
                else
                {
                    _logger.LogWarning("Job GitHub API returned no commits. Updating sync time.");
                    _cache.UpdateCommits(existingCommits);
                }

                _logger.LogInformation("Job 'GitHubCommitJob' completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during scheduled 'GitHubCommitJob' execution.");
            }
        }
    }
}
