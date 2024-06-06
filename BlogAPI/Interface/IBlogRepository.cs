using BlogAPI.Models;

namespace BlogAPI.Interface
{
    public interface IBlogRepository
    {
        ICollection<Blog> GetBlogs();
        Blog GetBlog(int blogId);
        bool CreateBlog(Blog blog);
        bool UpdateBlog(Blog blog);
        bool DeleteBlog(Blog blog);
        bool Save();
        bool BlogExists(int blogId);
    }
}
