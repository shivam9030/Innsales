 namespace InnSales.Domain.Entities
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PublishedBy { get; set; }
        public DateTime PublishedDate { get; set; }
        public string? EditedBy { get; set; }
        public DateTime? EditedDate { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string Category { get; set; }
        
        public string? SourceUrl { get; set; }
        
    }
}
