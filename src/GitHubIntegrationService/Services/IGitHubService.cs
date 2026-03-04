using CoreWCF;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubIntegrationService.Models;

namespace GitHubIntegrationService.Services
{
    [ServiceContract]
    public interface IGitHubService
    {
        [OperationContract]
        Task<List<GitHubCommit>> GetLiveCommits(string searchText, System.DateTime? startDate, System.DateTime? endDate, int page, int pageSize);

        [OperationContract]
        Task<int> GetLiveCommitsCount(string searchText, System.DateTime? startDate, System.DateTime? endDate);

        [OperationContract]
        List<GitHubCommit> GetScheduledCommits(string searchText, System.DateTime? startDate, System.DateTime? endDate, int page, int pageSize);

        [OperationContract]
        int GetScheduledCommitsCount(string searchText, System.DateTime? startDate, System.DateTime? endDate);

        [OperationContract]
        System.DateTime GetLastSyncTime();
    }
}
