using BL.Models;
using BL.DTO;
using DARE_FrontEnd.Services.Project;
using System.Text.Json;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http.HttpResults;
using static DARE_FrontEnd.Controllers.FormsController;

namespace DARE_FrontEnd.Services
{
    public class ProjectsHandler : IProjectsHandler
    {
        // public readonly IProjectsHandler _projectHandler;
        private readonly IClientHelper _clientHelper;
        private readonly IAPICaller _apiCaller;

        public ProjectsHandler( IAPICaller IApiCaller, IClientHelper clientHelper)
        {
            _clientHelper = clientHelper;
            
            _apiCaller = IApiCaller;
        }

        public async Task<BL.Models.Project> CreateProjectSettings(BL.Models.Project model)
        {
            //var jsonString = GetStringContent(model);
            ////serialising the model to be passed to the API
            //return await GenericGetData<DatasetMirrorSetting>("api/DatasetMirror/Save_Mirroring", HttpMethod.Post,
            //    jsonString);
            return model;

        }

        public async Task<BL.Models.Project> CreateProject(data model)
        {
            try
            {
                var stringContent = _clientHelper.GetStringContent(model);
                var result = await _clientHelper.GenericHttpRequestWithReturnType<BL.Models.Project>("/api/Project/Save_Project", stringContent);

                return result;
            }
            catch (Exception ex)
            {

                Console.WriteLine("An error occurred: " + ex.Message);
                throw;
            }

        }
        public async Task<User> AddAUser(FormIoData model)
        {
            try
            {
                var stringContent = _clientHelper.GetStringContent(model);
                var  result = await _clientHelper.GenericHttpRequestWithReturnType<User>("/api/User/AddUser", stringContent);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                throw;
            }
        }

        public async Task<BL.Models.Project> GetProjectSettings(int id)
        {
            var request = new RestRequest($"https://localhost:7058/api/Project/Get_Project/{id}", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(id), ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<BL.Models.Project>(request);
            return test.Data;
        }

        public async Task<BL.Models.Project> GetAllProjects()
        {
            var request = new RestRequest($"https://localhost:7163/api/Project/Get_AllProjects", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<BL.Models.Project>(request);
            return test.Data;
        }

        public async Task<User> GetAllUsers()
        {
            var request = new RestRequest($"https://localhost:7058/api/Project/Get_AllUsers", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<User>(request);
            return test.Data;
        }
     

        public async Task<User> GetAUser(int id)
        {
            var request = new RestRequest($"https://localhost:7058/api/User/Get_User/{id}", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(id), ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<User>(request);
            return test.Data;
        }

        //public async Task<ProjectMembership> AddMembership(ProjectMembership membership)
        //{
        //    var request = new RestRequest("https://localhost:7058/api/Project/Add_Membership", Method.Post);
        //    request.Method = Method.Post;
        //    request.AddHeader("Accept", "application/json");
        //    request.AddParameter("application/json", JsonConvert.SerializeObject(membership), ParameterType.RequestBody);
        //    var test = _apiCaller.Client.Execute<ProjectMembership>(request);
        //    return test.Data;

        //}
        //public async Task<ProjectMembership> GetAllMemberships()
        //{
        //    var request = new RestRequest($"https://localhost:7058/api/Project/Get_AllMemberships", Method.Get);
        //    request.Method = Method.Get;
        //    request.AddHeader("Accept", "application/json");
        //    request.AddParameter("application/json", ParameterType.RequestBody);
        //    var test = _apiCaller.Client.Execute<ProjectMembership>(request);
        //    return test.Data;
        //}

        public async Task<BL.Models.Endpoint> GetAllEndPoints(int projectId)
        {
            var request = new RestRequest($"https://localhost:7058/api/Project/Get_AllEndPoints/{projectId}", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<BL.Models.Endpoint>(request);
            return test.Data;
        }

        public Task<bool> AddAsync(BL.Models.Project ProjectModel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(BL.Models.Project ProjectModel)
        {
            throw new NotImplementedException();
        }

        public Task<T> GenericGetData<T>(string endPoint, StringContent jsonString = null, bool usePut = false) where T : class, new()
        {
            //return JSonConvert.Deserilaisable<T>(model)
            throw new NotImplementedException();
        }

        public Task<T> GenericGetData<T>(string v, Task<T> stringContent)
        {
            throw new NotImplementedException();
        }

    }
}
