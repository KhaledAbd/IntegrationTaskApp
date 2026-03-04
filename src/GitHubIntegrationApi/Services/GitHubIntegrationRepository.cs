using GitHubIntegrationApi.Models;
using GitHubIntegrationApi.Services.Wcf;
using System.ServiceModel;

namespace GitHubIntegrationApi.Services
{
    public interface IGitHubIntegrationRepository
    {
        Task<LiveCommitsDto> GetLiveCommitsAsync();
        Task<ScheduledCommitsDto> GetScheduledCommitsAsync(string searchText, DateTime? startDate, DateTime? endDate, int page, int pageSize);
    }

    public class GitHubIntegrationRepository : IGitHubIntegrationRepository
    {
        private readonly string _wcfServiceUrl;
        private readonly long _maxReceivedMessageSize;
        private readonly ILogger<GitHubIntegrationRepository> _logger;

        public GitHubIntegrationRepository(IConfiguration configuration, ILogger<GitHubIntegrationRepository> logger)
        {
            _wcfServiceUrl = configuration["WcfServiceUrl"] ?? "http://github-integration-service:8080/GitHubService.svc";
            _maxReceivedMessageSize = configuration.GetValue<long>("MaxReceivedMessageSize", 2147483647);
            _logger = logger;
        }

        private IGitHubService CreateChannel()
        {
            var binding = new BasicHttpBinding();
            binding.MaxReceivedMessageSize = _maxReceivedMessageSize; // To ensure large responses are supported
            var endpoint = new EndpointAddress(_wcfServiceUrl);
            var factory = new ChannelFactory<IGitHubService>(binding, endpoint);
            return factory.CreateChannel();
        }

        public async Task<LiveCommitsDto> GetLiveCommitsAsync()
        {
            _logger.LogInformation("Calling WCF GetLiveCommits and GetLiveCommitsCount...");
            var channel = CreateChannel();
            try
            {
                var commitsTask = channel.GetLiveCommits();
                
                return new LiveCommitsDto
                {
                    Items = MapCommits(await commitsTask, "Live"),
                    TotalCount = (await commitsTask).Count
                };
            }
            finally
            {
                (channel as IClientChannel)?.Close();
            }
        }

        public async Task<ScheduledCommitsDto> GetScheduledCommitsAsync(string searchText, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            _logger.LogInformation("Calling WCF GetScheduledCommits and GetLastSyncTime...");
            var channel = CreateChannel();
            try
            {
                var commitsTask = Task.Run(() => channel.GetScheduledCommits(searchText, startDate, endDate, page, pageSize));
                var countTask = Task.Run(() => channel.GetScheduledCommitsCount(searchText, startDate, endDate));
                var syncTimeTask = Task.Run(() => channel.GetLastSyncTime());
                
                await Task.WhenAll(commitsTask, countTask, syncTimeTask);
                
                return new ScheduledCommitsDto
                {
                    Items = MapCommits(await commitsTask, "Scheduled"),
                    TotalCount = await countTask,
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
