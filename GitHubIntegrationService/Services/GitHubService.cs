using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GitHubIntegrationService.Models;
using System.Linq;

namespace GitHubIntegrationService.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly ILogger<GitHubService> _logger;
        private readonly HttpClient _httpClient;
        private readonly CommitCache _cache;
        private readonly string _token;
        private readonly string _repoUrl;
        private readonly string _userAgent;

        public GitHubService(ILogger<GitHubService> logger, HttpClient httpClient, CommitCache cache, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            
            _token = configuration["GitHub:Token"] ?? string.Empty;
            _repoUrl = configuration["GitHub:RepoUrl"] ?? string.Empty;
            _userAgent = configuration["GitHub:UserAgent"] ?? "IntegrationTaskApp";

            _logger.LogInformation("GitHubService initialized for Repository: {RepoUrl}", _repoUrl);
        }

        public async Task<List<GitHubCommit>> GetLiveCommits()
        {
            _logger.LogInformation("Received request for Live Commits.");

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _repoUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                request.Headers.UserAgent.ParseAdd(_userAgent);

                _logger.LogInformation("Sending request to GitHub API...");
                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("GitHub API request failed with Status Code: {StatusCode}", response.StatusCode);
                    response.EnsureSuccessStatusCode();
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var commits = JsonSerializer.Deserialize<List<GitHubCommit>>(content, options);

                var result = commits?.Take(10).ToList() ?? new List<GitHubCommit>();
                _logger.LogInformation("Successfully retrieved {Count} live commits from GitHub.", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching live commits from GitHub.");
                throw;
            }
        }

        public List<GitHubCommit> GetScheduledCommits()
        {
            _logger.LogInformation("Received request for Scheduled Commits from cache.");
            var commits = _cache.GetCommits();
            _logger.LogInformation("Returning {Count} commits from the cache.", commits.Count);
            return commits;
        }

        public DateTime GetLastSyncTime()
        {
            _logger.LogInformation("Received request for LastSyncTime.");
            var lastSyncTime = _cache.GetLastSyncTime();
            _logger.LogInformation("Returning LastSyncTime: {LastSyncTime}", lastSyncTime);
            return lastSyncTime;
        }
    }
}
