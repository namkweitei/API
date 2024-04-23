namespace API.Models
{
    public class UploadedDocument
    {
        public int Id { get; set; }
        public int ContributionId { get; set; }
        public string Title { get; set; }
        public byte[] Content { get; set; } 
        public string ContentType { get; set; } 
        public Contribution Contribution { get; set; }
    }
}
