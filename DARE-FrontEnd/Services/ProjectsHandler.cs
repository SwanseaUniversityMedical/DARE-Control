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

        public async Task<Projects> CreateProjectSettings(Projects model)
        {
            //var jsonString = GetStringContent(model);
            ////serialising the model to be passed to the API
            //return await GenericGetData<DatasetMirrorSetting>("api/DatasetMirror/Save_Mirroring", HttpMethod.Post,
            //    jsonString);
            return model;

        }

        public async Task<Projects> CreateProject(data model)
        {
            try
            {
                var stringContent = _clientHelper.GetStringContent(model);
                var result = await _clientHelper.GenericHttpRequestWithReturnType<Projects>("/api/Project/Save_Project", stringContent);

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

        public async Task<Projects> GetProjectSettings(int id)
        {
            var request = new RestRequest($"https://localhost:7058/api/Project/Get_Project/{id}", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(id), ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<Projects>(request);
            return test.Data;
        }

        public async Task<Projects> GetAllProjects()
        {
            var request = new RestRequest($"https://localhost:7163/api/Project/Get_AllProjects", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<Projects>(request);
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

        public async void GetNewToken(int id)
        {
            var request = new RestRequest($"https://localhost:7058/api/User/GetNewToken/{id}", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(id), ParameterType.RequestBody);
            _apiCaller.Client.Execute(request);
        }

        public async Task<ProjectMembership> AddMembership(ProjectMembership membership)
        {
            var request = new RestRequest("https://localhost:7058/api/Project/Add_Membership", Method.Post);
            request.Method = Method.Post;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(membership), ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<ProjectMembership>(request);
            return test.Data;

        }
        public async Task<ProjectMembership> GetAllMemberships()
        {
            var request = new RestRequest($"https://localhost:7058/api/Project/Get_AllMemberships", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<ProjectMembership>(request);
            return test.Data;
        }

        public Task<bool> AddAsync(Projects ProjectModel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(Projects ProjectModel)
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
