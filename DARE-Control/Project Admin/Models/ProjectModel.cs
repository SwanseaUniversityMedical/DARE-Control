namespace Project_Admin.Models
{
    public class Project
    {
        public int Id { get; set; }
        public List<Users> Users { get; set; }
        public string Name { get; set; }

    }

  

    public class ProjectListModel
    {
        public List<Project> Projects { get; set; }
    }
}
