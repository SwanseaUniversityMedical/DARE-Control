using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.ViewModels
{
    public class SubmissionInfo
    {
        
        public string GetALlIDs()
        {

            var data = $"Submission ID: {Submission.Id.ToString()}, Sub ID(s) of ";
            foreach(var Sub in Submission.Children)
            {
                data = data + (Sub.Id.ToString() + " ");
            }
            return data;
        }

        public Submission Submission { get; set; }
        public Stages Stages { get; set; }


        
    }
}
