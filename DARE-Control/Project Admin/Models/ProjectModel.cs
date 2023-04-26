namespace Project_Admin.Models
{
    public class Project
    {
        public int Id { get; set; }
        public List<User> Users { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

  

    public class ProjectListModel
    {
        public List<Project> Projects { get; set; }
    }
}
