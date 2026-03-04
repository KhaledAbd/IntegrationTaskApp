using System;
using System.Collections.Generic;

namespace GitHubIntegrationApi.Models
{
    public class ScheduledCommitsDto : PagedResult<GitHubCommitDto>
    {
        public DateTime LastSyncTime { get; set; }
    }
}
