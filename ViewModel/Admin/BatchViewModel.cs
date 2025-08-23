namespace GCTConnect.ViewModel.Admin
{
    public class BatchViewModel
    {
        public int BatchId { get; set; }
        public int BatchYear { get; set; }
        public string DepartmentName { get; set; }
        public int? StudentsCount { get; set; }
        public bool IsActive { get; set; }
        public int DepartmentCount { get; internal set; }
    }
}
