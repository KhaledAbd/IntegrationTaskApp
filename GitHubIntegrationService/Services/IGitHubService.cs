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
        Task<List<GitHubCommit>> GetLiveCommits();

        [OperationContract]
        List<GitHubCommit> GetScheduledCommits();
    }
}
