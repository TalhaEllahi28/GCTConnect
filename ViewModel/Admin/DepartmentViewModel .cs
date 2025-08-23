namespace GCTConnect.ViewModel.Admin
{
    public class DepartmentViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string HodName { get; set; }
        public int StudentsCount { get; set; }
        public int TeachersCount { get; set; }
        public string IconClass { get; set; } // e.g. "fas fa-laptop-code"
    }
}
