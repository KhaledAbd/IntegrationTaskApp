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
        public async Task<ActionResult<ApiResponse<LiveCommitsDto>>> GetLive([FromQuery] string? searchText, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("GET /api/github/live requested.");
            var result = await _repository.GetLiveCommitsAsync(searchText ?? string.Empty, startDate, endDate, page, pageSize);
            return Ok(ApiResponse<LiveCommitsDto>.Ok(result, "Successfully retrieved live commits."));
        }

        [HttpGet("scheduled")]
        public async Task<ActionResult<ApiResponse<ScheduledCommitsDto>>> GetScheduled([FromQuery] string? searchText, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("GET /api/github/scheduled requested.");
            var result = await _repository.GetScheduledCommitsAsync(searchText ?? string.Empty, startDate, endDate, page, pageSize);
            return Ok(ApiResponse<ScheduledCommitsDto>.Ok(result, "Successfully retrieved scheduled commits from cache."));
        }
    }
}
