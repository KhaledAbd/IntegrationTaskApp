using System.ServiceModel;
using System.Runtime.Serialization;

namespace GitHubIntegrationApi.Services.Wcf
{
    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface IGitHubService
    {
        [OperationContract]
        Task<List<GitHubWcfCommit>> GetLiveCommits();

        [OperationContract]
        List<GitHubWcfCommit> GetScheduledCommits();

        [OperationContract]
        DateTime GetLastSyncTime();
    }

    [DataContract(Name = "GitHubCommit", Namespace = "http://schemas.datacontract.org/2004/07/GitHubIntegrationService.Models")]
    public class GitHubWcfCommit
    {
        [DataMember]
        public string? Sha { get; set; }

        [DataMember]
        public WcfCommitInfo? Commit { get; set; }

        [DataMember]
        public string? HtmlUrl { get; set; }
    }

    [DataContract(Name = "CommitInfo", Namespace = "http://schemas.datacontract.org/2004/07/GitHubIntegrationService.Models")]
    public class WcfCommitInfo
    {
        [DataMember]
        public WcfAuthorInfo? Author { get; set; }

        [DataMember]
        public string? Message { get; set; }
    }

    [DataContract(Name = "AuthorInfo", Namespace = "http://schemas.datacontract.org/2004/07/GitHubIntegrationService.Models")]
    public class WcfAuthorInfo
    {
        [DataMember]
        public string? Name { get; set; }

        [DataMember]
        public string? Email { get; set; }

        [DataMember]
        public DateTime Date { get; set; }
    }
}
