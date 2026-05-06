using Application.Dtos.Post;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace JujuApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PostBulkCopyController : ControllerBase
    {
        private readonly CreateBulkPostsUseCase _useCase;

        public PostBulkCopyController(CreateBulkPostsUseCase useCase)
        {
            _useCase = useCase;
        }

        [EnableRateLimiting("bulkPolicy")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [HttpPost("PostBulkCopy")]
        public async Task<IActionResult> Bulk(List<CreatePostDto> posts)
        {
            await _useCase.ExecuteAsync(posts);

            return Ok(new
            {
                message = "Bulk exitoso",
                count = posts.Count
            });
        }
    }
}
