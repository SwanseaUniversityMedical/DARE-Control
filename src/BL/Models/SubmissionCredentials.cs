﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BL.Models
{
    public class SubmissionCredentials: BaseModel
    {
        public int Id { get; set; }

        [DisplayName("Enter Submission Keycloak username")]
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; }
        [DisplayName("Enter Submission Keycloak password")]
        [Required(ErrorMessage = "Password is required.")]
        public string PasswordEnc { get; set; }

        [NotMapped]
        [DisplayName("Confirm Keycloak password")]
        [Compare("PasswordEnc", ErrorMessage = "Confirm password doesn't match, Type again!")]
        public string ConfirmPassword { get; set; }

        [NotMapped]
        
        public bool Valid { get; set; }
    }
}