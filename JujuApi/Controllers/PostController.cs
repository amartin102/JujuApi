using Application.Common.GenericResponse;
using Application.Dtos.Post;
using Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JujuApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet("GetByCustomerId/{customerId}")]
        public async Task<ActionResult<GenericResponse<List<GetPostDto>>>> GetByCustomerId(int customerId)
        {
            try
            {
                var posts = await _postService.GetByCustomerId(customerId);

                if (posts.Success)
                    return Ok(posts);

                if (posts.StatusCode.HasValue)
                    return StatusCode(posts.StatusCode.Value, posts);

                return NotFound(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GenericResponse<List<GetPostDto>> { Message = $"Error interno del servidor. {ex.Message}", Success = false });
            }
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<GenericResponse<IQueryable<GetPostDto>>>> GetAll()
        {
            try
            {
                var posts = await _postService.GetAll();

                if (posts.Success)
                    return Ok(posts);

                if (posts.StatusCode.HasValue)
                    return StatusCode(posts.StatusCode.Value, posts);

                return BadRequest(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GenericResponse<IQueryable<GetPostDto>> { Message = $"Error interno del servidor. {ex.Message}", Success = false });
            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult<GenericResponse<GetPostDto>>> Create(CreatePostDto postDto)
        {
            try
            {
                var createdPost = await _postService.Create(postDto);

                if (createdPost.Success)
                    return CreatedAtAction(nameof(GetByCustomerId), new { customerId = createdPost.Data?.CustomerId }, createdPost);

                if (createdPost.StatusCode.HasValue)
                    return StatusCode(createdPost.StatusCode.Value, createdPost);

                return BadRequest(createdPost);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GenericResponse<GetPostDto> { Message = $"Error interno del servidor. {ex.Message}", Success = false });
            }
        }

        [HttpPut("Update")]
        public async Task<ActionResult<GenericResponse<GetPostDto>>> Update(UpdatePostDto postDto)
        {
            try
            {
                var updateResult = await _postService.Update(postDto);

                var response = new GenericResponse<GetPostDto>
                {
                    Success = updateResult.Success,
                    Message = updateResult.Message,
                    StatusCode = updateResult.StatusCode,
                    Data = updateResult.Data.updatedPost
                };

                if (response.StatusCode.HasValue)
                {
                    if (response.StatusCode.Value == StatusCodes.Status204NoContent)
                        return NoContent();

                    return StatusCode(response.StatusCode.Value, response);
                }

                return response.Success ? Ok(response) : NotFound(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GenericResponse<GetPostDto> { Message = $"Error interno del servidor. {ex.Message}", Success = false });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> Delete(int id)
        {
            try
            {
                var deleteResult = await _postService.Delete(id);
               
                if (deleteResult.StatusCode.HasValue)
                {
                    if (deleteResult.StatusCode.Value == StatusCodes.Status204NoContent)
                        return NoContent();
                    return StatusCode(deleteResult.StatusCode.Value, deleteResult);
                }

                return deleteResult.Success ? Ok(deleteResult) : BadRequest(deleteResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GenericResponse<bool> { Message = $"Error interno del servidor. {ex.Message}", Success = false });
            }
        }
    }
}
