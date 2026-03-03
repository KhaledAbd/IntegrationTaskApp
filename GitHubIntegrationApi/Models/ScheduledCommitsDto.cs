using System;
using System.Collections.Generic;

namespace GitHubIntegrationApi.Models
{
    public class ScheduledCommitsDto
    {
        public List<GitHubCommitDto> Commits { get; set; } = new();
        public DateTime LastSyncTime { get; set; }
    }
}
