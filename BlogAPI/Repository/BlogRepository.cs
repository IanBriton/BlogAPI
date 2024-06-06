using BlogAPI.Data;
using BlogAPI.Interface;
using BlogAPI.Models;

namespace BlogAPI.Repository
{
    public class BlogRepository : IBlogRepository
    {
        private readonly DataContext _context;

        public BlogRepository(DataContext context)
        {
           _context = context;
        }

        public bool BlogExists(int blogId)
        {
            return _context.Blogs.Any(b => b.Id == blogId);
        }

        public bool CreateBlog(Blog blog)
        {
            _context.Add(blog);
            return Save();
        }

        public bool DeleteBlog(Blog blog)
        {
            _context.Remove(blog);
            return Save();
        }

        public Blog GetBlog(int blogId)
        {
            return _context.Blogs.FirstOrDefault(b => b.Id == blogId);
        }

        public ICollection<Blog> GetBlogs()
        {
            return _context.Blogs.ToList();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0;
        }

        public bool UpdateBlog(Blog blog)
        {
            _context.Update(blog);
            return Save();
        }
    }
}
