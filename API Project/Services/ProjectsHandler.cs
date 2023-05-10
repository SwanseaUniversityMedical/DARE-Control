using Project_Admin.Models;
namespace API_Project.Services
{
    public class ProjectsHandler
    {

        public async Task<Projects> CreateProjectSettings(Projects model)
        {
            //var jsonString = GetStringContent(model);
            ////serialising the model to be passed to the API
            //return await GenericGetData<DatasetMirrorSetting>("api/DatasetMirror/Save_Mirroring", HttpMethod.Post,
            //    jsonString);
            return model;

        }
    }
}
