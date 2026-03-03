using GitHubIntegrationApi.Models;
using GitHubIntegrationApi.Services.Wcf;
using System.ServiceModel;

namespace GitHubIntegrationApi.Services
{
    public interface IGitHubIntegrationRepository
    {
        Task<List<GitHubCommitDto>> GetLiveCommitsAsync();
        Task<ScheduledCommitsDto> GetScheduledCommitsAsync();
    }

    public class GitHubIntegrationRepository : IGitHubIntegrationRepository
    {
        private readonly string _wcfServiceUrl;
        private readonly ILogger<GitHubIntegrationRepository> _logger;

        public GitHubIntegrationRepository(IConfiguration configuration, ILogger<GitHubIntegrationRepository> logger)
        {
            _wcfServiceUrl = configuration["WcfServiceUrl"] ?? "http://github-integration-service:8080/GitHubService.svc";
            _logger = logger;
        }

        private IGitHubService CreateChannel()
        {
            var binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress(_wcfServiceUrl);
            var factory = new ChannelFactory<IGitHubService>(binding, endpoint);
            return factory.CreateChannel();
        }

        public async Task<List<GitHubCommitDto>> GetLiveCommitsAsync()
        {
            _logger.LogInformation("Calling WCF GetLiveCommits...");
            var channel = CreateChannel();
            try
            {
                var commits = await channel.GetLiveCommits();
                return MapCommits(commits, "Live");
            }
            finally
            {
                (channel as IClientChannel)?.Close();
            }
        }

        public async Task<ScheduledCommitsDto> GetScheduledCommitsAsync()
        {
            _logger.LogInformation("Calling WCF GetScheduledCommits and GetLastSyncTime...");
            var channel = CreateChannel();
            try
            {
                var commitsTask = Task.Run(() => channel.GetScheduledCommits());
                var syncTimeTask = Task.Run(() => channel.GetLastSyncTime());
                
                await Task.WhenAll(commitsTask, syncTimeTask);
                
                return new ScheduledCommitsDto
                {
                    Commits = MapCommits(await commitsTask, "Scheduled"),
                    LastSyncTime = await syncTimeTask
                };
            }
            finally
            {
                (channel as IClientChannel)?.Close();
            }
        }

        private List<GitHubCommitDto> MapCommits(List<GitHubWcfCommit> wcfCommits, string source)
        {
            if (wcfCommits == null) return new List<GitHubCommitDto>();

            return wcfCommits.Select(c => new GitHubCommitDto
            {
                Sha = c.Sha,
                AuthorName = c.Commit?.Author?.Name,
                AuthorEmail = c.Commit?.Author?.Email,
                Date = c.Commit?.Author?.Date ?? DateTime.MinValue,
                Message = c.Commit?.Message,
                HtmlUrl = c.HtmlUrl,
                Source = source
            }).ToList();
        }
    }
}
