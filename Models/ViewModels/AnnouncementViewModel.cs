namespace GCTConnect.Models.ViewModels
{
    public class AnnouncementViewModel
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Priority { get; set; }
        public string Audience { get; set; }
    }

}
