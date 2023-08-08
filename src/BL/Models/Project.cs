﻿using BL.Models.DTO;
using Newtonsoft.Json;

namespace BL.Models
{
    public class Project
    {
        public int Id { get; set; }
        
        public virtual List<User> Users { get; set; }

        public virtual List<Endpoint> Endpoints { get; set; }
        public string FormData { get; set; }
        public string Name { get; set; }
        public string Display { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? SubmissionBucket { get; set; }
        public string? OutputBucket { get; set; }

        //public virtual List<ProjectMembership> ProjectMemberships { get; set; }

        public virtual List<Submission> Submissions { get; set; }

    }



    public class ProjectListModel
    {
        public List<Project> Projects { get; set; }

    }
}
