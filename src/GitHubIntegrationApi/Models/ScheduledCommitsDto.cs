using System;
using System.Collections.Generic;

namespace GitHubIntegrationApi.Models
{
    public class ScheduledCommitsDto
    {
        public List<GitHubCommitDto> Commits { get; set; } = new();
        public int TotalCount { get; set; }
        public DateTime LastSyncTime { get; set; }
    }
}
