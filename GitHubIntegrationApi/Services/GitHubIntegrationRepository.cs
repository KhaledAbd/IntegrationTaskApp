using GitHubIntegrationApi.Models;
using GitHubIntegrationApi.Services.Wcf;
using System.ServiceModel;

namespace GitHubIntegrationApi.Services
{
    public interface IGitHubIntegrationRepository
    {
        Task<List<GitHubCommitDto>> GetLiveCommitsAsync();
        Task<List<GitHubCommitDto>> GetScheduledCommitsAsync();
    }

    public class GitHubIntegrationRepository : IGitHubIntegrationRepository
    {
        private readonly string _wcfServiceUrl;
        private readonly ILogger<GitHubIntegrationRepository> _logger;

        public GitHubIntegrationRepository(IConfiguration configuration, ILogger<GitHubIntegrationRepository> logger)
        {
            _wcfServiceUrl = configuration["WcfServiceUrl"] ?? "http://localhost:5000/GitHubService.svc";
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
                return MapCommits(commits);
            }
            finally
            {
                (channel as IClientChannel)?.Close();
            }
        }

        public async Task<List<GitHubCommitDto>> GetScheduledCommitsAsync()
        {
            _logger.LogInformation("Calling WCF GetScheduledCommits...");
            var channel = CreateChannel();
            try
            {
                // WCF method is synchronous in interface definition (List<GitHubCommit> GetScheduledCommits())
                var commits = await Task.Run(() => channel.GetScheduledCommits());
                return MapCommits(commits);
            }
            finally
            {
                (channel as IClientChannel)?.Close();
            }
        }

        private List<GitHubCommitDto> MapCommits(List<GitHubWcfCommit> wcfCommits)
        {
            if (wcfCommits == null) return new List<GitHubCommitDto>();

            return wcfCommits.Select(c => new GitHubCommitDto
            {
                Sha = c.Sha,
                AuthorName = c.Commit?.Author?.Name,
                AuthorEmail = c.Commit?.Author?.Email,
                Date = c.Commit?.Author?.Date ?? DateTime.MinValue,
                Message = c.Commit?.Message,
                HtmlUrl = c.HtmlUrl
            }).ToList();
        }
    }
}
