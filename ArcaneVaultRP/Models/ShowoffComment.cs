namespace ArcaneVaultRP.Models
{
    public class ShowoffComment
    {
        public int CommentId { get; set; }
        public int ShowoffId { get; set; }
        public string UserName { get; set; }
        public string? AvatarUrl { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
