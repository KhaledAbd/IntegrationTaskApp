using GitHubIntegrationApi.Models;
using GitHubIntegrationApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GitHubIntegrationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase
    {
        private readonly IGitHubIntegrationRepository _repository;
        private readonly ILogger<GitHubController> _logger;

        public GitHubController(IGitHubIntegrationRepository repository, ILogger<GitHubController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("live")]
        public async Task<ActionResult<ApiResponse<List<GitHubCommitDto>>>> GetLive()
        {
            _logger.LogInformation("GET /api/github/live requested.");
            var commits = await _repository.GetLiveCommitsAsync();
            return Ok(ApiResponse<List<GitHubCommitDto>>.Ok(commits, "Successfully retrieved live commits."));
        }

        [HttpGet("scheduled")]
        public async Task<ActionResult<ApiResponse<List<GitHubCommitDto>>>> GetScheduled()
        {
            _logger.LogInformation("GET /api/github/scheduled requested.");
            var commits = await _repository.GetScheduledCommitsAsync();
            return Ok(ApiResponse<List<GitHubCommitDto>>.Ok(commits, "Successfully retrieved scheduled commits from cache."));
        }
    }
}
