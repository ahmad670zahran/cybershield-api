namespace CyberShield.API.Models
{
    public class SecurityNews
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishedAt { get; set; } = DateTime.Now;
    }

    public class SecurityTip
    {
        public int Id { get; set; }
        public string Title { get; set; }   
        public string Description { get; set; }    
    }
}
