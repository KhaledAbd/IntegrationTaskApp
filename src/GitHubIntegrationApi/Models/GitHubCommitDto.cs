namespace GitHubIntegrationApi.Models
{
    public class GitHubCommitDto
    {
        public string? Sha { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorEmail { get; set; }
        public DateTime Date { get; set; }
        public string? Message { get; set; }
        public string? HtmlUrl { get; set; }
        public string? Source { get; set; }
    }
}
