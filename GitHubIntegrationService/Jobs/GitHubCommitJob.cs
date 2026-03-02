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
            try
            {
                _logger.LogInformation("Refreshing GitHub commits cache...");

                var request = new HttpRequestMessage(HttpMethod.Get, _repoUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                request.Headers.UserAgent.ParseAdd(_userAgent);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var commits = JsonSerializer.Deserialize<List<GitHubCommit>>(content, options);

                if (commits != null)
                {
                    _cache.UpdateCommits(commits.Take(10).ToList());
                    _logger.LogInformation("Successfully updated commit cache with {Count} commits.", commits.Take(10).Count());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating GitHub commits cache.");
            }
        }
    }
}
