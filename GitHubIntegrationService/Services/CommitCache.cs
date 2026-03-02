using System.Collections.Generic;
using GitHubIntegrationService.Models;

namespace GitHubIntegrationService.Services
{
    public class CommitCache
    {
        private List<GitHubCommit> _commits = new List<GitHubCommit>();
        private readonly object _lock = new object();

        public void UpdateCommits(List<GitHubCommit> commits)
        {
            lock (_lock)
            {
                _commits = commits ?? new List<GitHubCommit>();
            }
        }

        public List<GitHubCommit> GetCommits()
        {
            lock (_lock)
            {
                return new List<GitHubCommit>(_commits);
            }
        }
    }
}
