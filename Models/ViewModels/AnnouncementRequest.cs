namespace GCTConnect.Models.ViewModels
{
    public class AnnouncementRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Priority { get; set; }
        public string Audience { get; set; }
        public int? DepartmentId { get; set; }
        public int? BatchId { get; set; }
    }

}
