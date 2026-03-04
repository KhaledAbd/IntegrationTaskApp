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

        public GitHubService(ILogger<GitHubService> logger, HttpClient httpClient, CommitCache cache, Microsoft.Extensions.Options.IOptions<GitHubSettings> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            
            var settings = options.Value;
            _token = settings.Token;
            _repoUrl = settings.RepoUrl;
            _userAgent = string.IsNullOrWhiteSpace(settings.UserAgent) ? "IntegrationTaskApp" : settings.UserAgent;

            _logger.LogInformation("GitHubService initialized for Repository: {RepoUrl}", _repoUrl);
        }

        public async Task<List<GitHubCommit>> GetLiveCommits(string searchText, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            _logger.LogInformation("Received request for Live Commits. Search: {Search}, Start: {Start}, End: {End}, Page: {Page}, PageSize: {PageSize}", searchText, startDate, endDate, page, pageSize);

            try
            {
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"per_page={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    queryParams.Add($"author={Uri.EscapeDataString(searchText)}");
                }
                
                if (startDate.HasValue)
                {
                    queryParams.Add($"since={startDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
                }

                if (endDate.HasValue)
                {
                    queryParams.Add($"until={endDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
                }

                var url = $"{_repoUrl}?{string.Join("&", queryParams)}";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
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

                var result = commits ?? new List<GitHubCommit>();
                _logger.LogInformation("Successfully retrieved {Count} live commits from GitHub.", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching live commits from GitHub.");
                throw;
            }
        }

        public List<GitHubCommit> GetScheduledCommits(string searchText, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            _logger.LogInformation("Received request for Scheduled Commits from cache. Search: {Search}, Start: {Start}, End: {End}, Page: {Page}, PageSize: {PageSize}", searchText, startDate, endDate, page, pageSize);
            var commits = _cache.GetCommits();

            var query = commits.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(c => (c.Commit?.Author?.Name != null && c.Commit.Author.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                                         (c.Commit?.Author?.Email != null && c.Commit.Author.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
            }
            if (startDate.HasValue)
            {
                query = query.Where(c => c.Commit?.Author?.Date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(c => c.Commit?.Author?.Date <= endDate.Value);
            }
            
            var result = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            _logger.LogInformation("Returning {Count} commits from the cache for current page.", result.Count);
            return result;
        }

        public int GetScheduledCommitsCount(string searchText, DateTime? startDate, DateTime? endDate)
        {
            var commits = _cache.GetCommits();
            var query = commits.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(c => (c.Commit?.Author?.Name != null && c.Commit.Author.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                                         (c.Commit?.Author?.Email != null && c.Commit.Author.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
            }
            if (startDate.HasValue)
            {
                query = query.Where(c => c.Commit?.Author?.Date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(c => c.Commit?.Author?.Date <= endDate.Value);
            }
            return query.Count();
        }

        public async Task<int> GetLiveCommitsCount(string searchText, DateTime? startDate, DateTime? endDate)
        {
            _logger.LogInformation("Received request for Live Commits Count using Commits API. Search: {Search}, Start: {Start}, End: {End}", searchText, startDate, endDate);

            try
            {
                var queryParams = new List<string> { "per_page=1" };

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    queryParams.Add($"author={Uri.EscapeDataString(searchText)}");
                }
                
                if (startDate.HasValue)
                {
                    queryParams.Add($"since={startDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
                }

                if (endDate.HasValue)
                {
                    queryParams.Add($"until={endDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
                }

                var url = $"{_repoUrl}?{string.Join("&", queryParams)}";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                request.Headers.UserAgent.ParseAdd(_userAgent);

                _logger.LogInformation("Sending request to GitHub Commits API for count: {Url}", url);
                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("GitHub API request failed with Status Code: {StatusCode}. Response: {Response}", response.StatusCode, errorContent);
                    response.EnsureSuccessStatusCode();
                }

                // GitHub's pagination for the commits API exposes the total via the 'Link' header
                // Reference: https://docs.github.com/en/rest/guides/using-pagination-in-the-rest-api
                if (response.Headers.TryGetValues("Link", out var linkValues))
                {
                    var linkHeader = string.Join(", ", linkValues);
                    var links = linkHeader.Split(',');
                    var lastLink = links.FirstOrDefault(l => l.Contains("rel=\"last\""));
                    
                    if (lastLink != null)
                    {
                        // Format: <url?page=N>; rel="last"
                        var match = System.Text.RegularExpressions.Regex.Match(lastLink, @"page=(\d+)");
                        if (match.Success && int.TryParse(match.Groups[1].Value, out var lastPage))
                        {
                            _logger.LogInformation("Extracted total count from Link header: {Count}", lastPage);
                            return lastPage;
                        }
                    }
                }

                // If no Link header, check if we have any items at all
                var content = await response.Content.ReadAsStringAsync();
                var commits = JsonSerializer.Deserialize<List<JsonElement>>(content);
                var count = commits?.Count ?? 0;
                
                _logger.LogInformation("No Link header found. Counted {Count} items in response.", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching live commits count from GitHub.");
                throw;
            }
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
