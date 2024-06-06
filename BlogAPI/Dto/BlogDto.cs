namespace BlogAPI.Dto
{
    public class BlogDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Body { get; set; }
    }
}
