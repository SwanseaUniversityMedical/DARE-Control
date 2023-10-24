using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BL.Models
{
    public static class AuthorizationPolicies
    {
        public static AuthorizationPolicy GetUserAllowedPolicy()
        {
            var policyBuilder = new AuthorizationPolicyBuilder(); 
            //add other policy requirements here

            policyBuilder.RequireClaim("user_in_tre");
            policyBuilder.RequireClaim("allow");
            policyBuilder.RequireClaim("project_allow");
            return policyBuilder.Build(); 
        }
    }
}