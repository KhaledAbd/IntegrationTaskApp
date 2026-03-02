using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using GitHubIntegrationService.Models;
using System.Linq;

namespace GitHubIntegrationService.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly CommitCache _cache;
        private readonly string _token;
        private readonly string _repoUrl;
        private readonly string _userAgent;

        public GitHubService(HttpClient httpClient, CommitCache cache, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            
            _token = configuration["GitHub:Token"] ?? string.Empty;
            _repoUrl = configuration["GitHub:RepoUrl"] ?? string.Empty;
            _userAgent = configuration["GitHub:UserAgent"] ?? "IntegrationTaskApp";
        }

        public async Task<List<GitHubCommit>> GetLiveCommits()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _repoUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Headers.UserAgent.ParseAdd(_userAgent);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var commits = JsonSerializer.Deserialize<List<GitHubCommit>>(content, options);

            return commits?.Take(10).ToList() ?? new List<GitHubCommit>();
        }

        public List<GitHubCommit> GetScheduledCommits()
        {
            return _cache.GetCommits();
        }
    }
}
