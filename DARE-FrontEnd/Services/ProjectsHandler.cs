﻿using BL.Models;
using DARE_FrontEnd.Services.Project;
using System.Text.Json;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace DARE_FrontEnd.Services
{
    public class ProjectsHandler : IProjectsHandler
    {
        // public readonly IProjectsHandler _projectHandler;
        private readonly IAPICaller _apiCaller;

        public ProjectsHandler( IAPICaller IApiCaller)
        {
            
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
        public async Task<Projects> CreateProject(Projects model)
        {
            var request = new RestRequest("https://localhost:7058/api/Project/Save_Project", Method.Post);
            request.Method = Method.Post;
            request.AddHeader("Accept", "application/json");
            //var li = request.Parameters.ToList();
            //foreach (var l in li)
            //{
            //    request.Parameters.RemoveParameter(l);
            //}
            //request.Parameters.Clear();
            request.AddParameter("application/json", JsonConvert.SerializeObject(model), ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<Projects>(request);
            return test.Data;
            //return JsonConvert.DeserializeObject<MoveProject>(test.Content);
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
        public async Task<User> AddAUser(User user)
        {
            var request = new RestRequest("https://localhost:7058/api/User/Add_User", Method.Post);
            request.Method = Method.Post;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(user), ParameterType.RequestBody);
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

        public async Task<User> GetNewToken(int id)
        {
            var request = new RestRequest($"https://localhost:7058/api/User/GetNewToken/{id}", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(id), ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<User>(request);
            return test.Data;
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

        //var stringContent = _projectHandler.CreateProject(model);
        //    //var jsonString = GetStringContent(model);
        //    return await GenericGetData<Projects>($"/api/ProjectController/Save_Project", stringContent);
        //}

        //private StringContent GetStringContent<T>(T datasetObj) where T : class
        //{
        //    var test = JsonSerializer.Serialize(datasetObj, _jsonSerializerOptions);
        //    var jsonString = new StringContent(
        //        JsonSerializer.Serialize(datasetObj, _jsonSerializerOptions),
        //        Encoding.UTF8,
        //        "application/json");
        //    return jsonString;
        //}

    }
}
