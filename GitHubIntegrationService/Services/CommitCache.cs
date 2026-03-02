using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using GitHubIntegrationService.Models;

namespace GitHubIntegrationService.Services
{
    public class CommitCache
    {
        private readonly ILogger<CommitCache> _logger;
        private List<GitHubCommit> _commits = new List<GitHubCommit>();
        private DateTime _lastSyncTime;
        private readonly object _lock = new object();
        private readonly string _cacheFilePath = "commit_cache.json";

        public CommitCache(ILogger<CommitCache> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("CommitCache initialized. Cache file path: {CacheFilePath}", _cacheFilePath);
            LoadFromDisk();
        }

        public void UpdateCommits(List<GitHubCommit> commits)
        {
            _logger.LogInformation("Updating CommitCache with {Count} commits.", commits?.Count ?? 0);
            lock (_lock)
            {
                _commits = commits ?? new List<GitHubCommit>();
                _lastSyncTime = DateTime.UtcNow;
                _logger.LogInformation("Cache memory updated at {Time}.", _lastSyncTime);
                SaveToDisk();
            }
        }

        public List<GitHubCommit> GetCommits()
        {
            _logger.LogDebug("Retrieving commits from cache memory.");
            lock (_lock)
            {
                return new List<GitHubCommit>(_commits);
            }
        }

        public DateTime GetLastSyncTime()
        {
            _logger.LogDebug("Retrieving LastSyncTime from cache memory.");
            lock (_lock)
            {
                return _lastSyncTime;
            }
        }

        private void SaveToDisk()
        {
            _logger.LogInformation("Serializing cache data to disk: {CacheFilePath}", _cacheFilePath);
            try
            {
                var data = new CacheData
                {
                    Commits = _commits,
                    LastSyncTime = _lastSyncTime
                };
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_cacheFilePath, json);
                _logger.LogInformation("Successfully saved cache data to disk.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save cache data to disk.");
            }
        }

        private void LoadFromDisk()
        {
            _logger.LogInformation("Attempting to load cache data from disk: {CacheFilePath}", _cacheFilePath);
            try
            {
                if (File.Exists(_cacheFilePath))
                {
                    var json = File.ReadAllText(_cacheFilePath);
                    var data = JsonSerializer.Deserialize<CacheData>(json);
                    if (data != null)
                    {
                        _commits = data.Commits ?? new List<GitHubCommit>();
                        _lastSyncTime = data.LastSyncTime;
                        _logger.LogInformation("Successfully loaded {Count} commits from disk. Last Sync Time was: {LastSyncTime}", _commits.Count, _lastSyncTime);
                    }
                }
                else
                {
                    _logger.LogInformation("No cache file found on disk. Starting with an empty cache.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load cache data from disk.");
            }
        }

        private class CacheData
        {
            public List<GitHubCommit> Commits { get; set; } = new List<GitHubCommit>();
            public DateTime LastSyncTime { get; set; }
        }
    }
}
