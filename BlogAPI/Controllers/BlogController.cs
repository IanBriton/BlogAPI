using AutoMapper;
using BlogAPI.Dto;
using BlogAPI.Dto.OtherObjects;
using BlogAPI.Interface;
using BlogAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : Controller
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IMapper _mapper;

        public BlogController(IBlogRepository blogRepository, IMapper mapper)
        {
            _blogRepository = blogRepository;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Blog>))]
        [Authorize(Roles = StaticUserRoles.ADMIN)]

        public IActionResult GetBlogs()
        {
            var blogs = _blogRepository.GetBlogs();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(blogs);
        }

        [HttpGet("blogId/{blogId}")]
        [ProducesResponseType(200, Type = typeof(Blog))]
        [ProducesResponseType(400)]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public IActionResult GetBlog(int blogId)
        {
            if (!_blogRepository.BlogExists(blogId))
                return NotFound();

            var blog = _mapper.Map<BlogDto>(_blogRepository.GetBlog(blogId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(blog);

        }

        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [Authorize(Roles =StaticUserRoles.USER)]
        public IActionResult CreateBlog([FromBody] BlogDto blogCreate)
        {
            if (blogCreate == null)
                return BadRequest(ModelState);
            var existingBlogs = _blogRepository.GetBlogs().FirstOrDefault(b => b.Title.Trim().ToUpper() == blogCreate.Title.TrimEnd().ToUpper());

            if (existingBlogs != null)
            {
                ModelState.AddModelError("", "A Blog with this title already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var blogMap = _mapper.Map<Blog>(blogCreate);
            if (!_blogRepository.CreateBlog(blogMap))
            {
                ModelState.AddModelError("", "Smoething went wrong while saving the blog");
                return StatusCode(500, ModelState);
            }

            return Ok("Succesfully created.");
        }

        [HttpPut("blogId/{blogId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [Authorize(Roles = StaticUserRoles.USER)]
        public IActionResult UpdateBlog(int blogId, [FromBody] BlogDto updatedBlog)
        {
            if (updatedBlog == null)
            {
                ModelState.AddModelError("", "Update Blog is null");
                return BadRequest(ModelState);
            }

            if (blogId != updatedBlog.Id)
            {
                ModelState.AddModelError("", "BlogId Mismatch");
                return BadRequest(ModelState);
            }
            if (!_blogRepository.BlogExists(blogId))
            {
                ModelState.AddModelError("", "Blog not found");
                return NotFound();
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var blogMap = _mapper.Map<Blog>(updatedBlog);

            if (!_blogRepository.UpdateBlog(blogMap))
            {
                ModelState.AddModelError("", "Something went wrong while updating the blog");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("blogId/{blogId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [Authorize(Roles =StaticUserRoles.OWNER)]
        public IActionResult DeleteBlog(int blogId)
        {
            if (!_blogRepository.BlogExists(blogId))
                return NotFound();

            var blogToDelete = _blogRepository.GetBlog(blogId);

            if (!_blogRepository.DeleteBlog(blogToDelete))
            {
                ModelState.AddModelError("", "Something went wrong while trying to delete the blog");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
