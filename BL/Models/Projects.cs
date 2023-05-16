namespace BL.Models
{
    public class Projects
    {
        public int Id { get; set; }
        public List<User> Users { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        
    }

  

    public class ProjectListModel
    {
        public List<Projects> Projects { get; set; }
    }
}
