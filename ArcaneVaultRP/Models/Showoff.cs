namespace ArcaneVaultRP.Models
{
    public class Showoff
    {
        public int ShowoffId { get; set; }
        public string ShowoffName { get; set; }
        public string BackgroundColor { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsDeleted { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public List<ShowoffItem> Items { get; set; } = new();
    }
}