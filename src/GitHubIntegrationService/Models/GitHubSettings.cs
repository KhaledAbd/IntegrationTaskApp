namespace GitHubIntegrationService.Models
{
    public class GitHubSettings
    {
        public string Token { get; set; } = string.Empty;
        public string RepoUrl { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
