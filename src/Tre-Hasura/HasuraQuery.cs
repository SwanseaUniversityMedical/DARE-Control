using BL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL.Models;
using BL.Models.ViewModels;

namespace Tre_Hasura
{
    public interface IHasuraQuery
    {
        void RunQuery(string token, string role, string Query);
    }
    public class HasuraQuery : IHasuraQuery    
    {
        public void RunQuery(string token, string role, string Query)
        {
            //dont need to check token as Hasura does this 
            //make query
            var projects = TREClientHelper.CallAPIWithoutModel<List<TreProject>>("/api/Approval/GetAllTreProjects/", paramlist).Result;

        }
    }
}
