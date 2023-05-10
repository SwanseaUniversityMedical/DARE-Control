using Project_Admin.Models;
using Project_Admin.Services.Project;

namespace Project_Admin.Services
{
    public class ProjectsHandler
    {
        public readonly IProjectsHandler _projectHandler;
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
            var stringContent = _projectHandler.CreateProject(model);

            return await _projectHandler.GenericGetData<Projects>($"/api/ProjectController/Save_Project", stringContent);
        }

    }
}
