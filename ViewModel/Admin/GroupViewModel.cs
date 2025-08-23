namespace GCTConnect.ViewModel.Admin
{
    public class GroupViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupType { get; set; }
        public string DepartmentName { get; set; }
        public int MembersCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
