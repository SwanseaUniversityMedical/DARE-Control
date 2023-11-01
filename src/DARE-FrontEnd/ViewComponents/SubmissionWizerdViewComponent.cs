using BL.Models.ViewModels;
using BL.Services;
using Microsoft.AspNetCore.Mvc;

namespace DARE_FrontEnd.ViewComponents
{
    public class SubmissionWizerdViewComponent : ViewComponent
    {
        private readonly IDareClientHelper _clientHelper;
        public SubmissionWizerdViewComponent(IDareClientHelper client)
        {
            _clientHelper = client;
        }
        public async Task<IViewComponentResult> InvokeAsync(int projectId)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", projectId.ToString());
            var project = _clientHelper.CallAPIWithoutModel<BL.Models.Project?>(
                "/api/Project/GetProject/", paramlist).Result;
            var SelectTresOptions = project.Tres.Select(x => new { Name = x.Name, LastHeartBeatReceived = x.LastHeartBeatReceived }).ToList();
            List<TreInfo> treInfoList = new List<TreInfo>();
            foreach (var param in SelectTresOptions)
            {
                TimeSpan timeSinceLastUpdate = DateTime.Now - param.LastHeartBeatReceived;
                var isOnline = false;
                if(timeSinceLastUpdate.TotalMinutes<30)
                    isOnline = true;

                var treInfo = new TreInfo()
                {
                    Name = param.Name,
                    IsSelected = false,
                    IsOnline = isOnline
                };
                treInfoList.Add(treInfo);
            }
            var model = new SubmissionWizard()
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                SelectTresOptions = project.Tres.Select(x => x.Name).ToList(),
                TreRadios = treInfoList
            };
            return View(model);
        }
    }
}
