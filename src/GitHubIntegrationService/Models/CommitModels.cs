using System.Runtime.Serialization;

namespace GitHubIntegrationService.Models
{
    [DataContract]
    public class GitHubCommit
    {
        [DataMember]
        public string? Sha { get; set; }

        [DataMember]
        public CommitInfo? Commit { get; set; }

        [DataMember]
        public string? HtmlUrl { get; set; }
    }

    [DataContract]
    public class CommitInfo
    {
        [DataMember]
        public AuthorInfo? Author { get; set; }

        [DataMember]
        public string? Message { get; set; }
    }

    [DataContract]
    public class AuthorInfo
    {
        [DataMember]
        public string? Name { get; set; }

        [DataMember]
        public string? Email { get; set; }

        [DataMember]
        public System.DateTime Date { get; set; }
    }
    [DataContract]
    public class GitHubSearchResult
    {
        [DataMember]
        public int TotalCount { get; set; }
        
        [DataMember]
        public bool IncompleteResults { get; set; }
        
        [DataMember]
        public List<GitHubCommit> Items { get; set; } = new();
    }
}
