using BL.Models.ViewModels;
using BL.Services;
using Microsoft.AspNetCore.Mvc;

namespace DARE_FrontEnd.ViewComponents
{
    public class SubmissionWizardRawViewComponent : ViewComponent
    {
        private readonly IDareClientHelper _clientHelper;
        public SubmissionWizardRawViewComponent(IDareClientHelper client)
        {
            _clientHelper = client;
        }
        public async Task<IViewComponentResult> InvokeAsync(int projectId)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", projectId.ToString());
            var project = _clientHelper.CallAPIWithoutModel<BL.Models.Project?>(
                "/api/Project/GetProject/", paramlist).Result;
            var SelectTresOptions = project.Tres.ToList();
            List<TreInfo> treInfoList = new List<TreInfo>();
            foreach (var param in SelectTresOptions)
            {
                var isOnline = param.IsOnline();

                var treInfo = new TreInfo()
                {
                    Name = param.Name,
                    IsSelected = false,
                    IsOnline = isOnline
                };
                treInfoList.Add(treInfo);
            }
            var model = new AddiSubmissionWizard()
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
