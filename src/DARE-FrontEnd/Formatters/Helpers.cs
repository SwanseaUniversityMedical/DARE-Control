using BL.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BL.Models.ViewModels;

namespace DARE_FrontEnd.Formatters
{
    public class Helpers
    {
        public static string PrettyStatus(StatusType enumValue)
        {
            //StatusType enumValue = StatusType.UserNotOnProject;
            var displayNameAtt = enumValue.GetType().GetMember(enumValue.ToString())[0].GetCustomAttribute<DisplayAttribute>();
            return displayNameAtt == null ? enumValue.ToString() : displayNameAtt.Name;
        }

        public static string PrettyCrateOrigin(CrateOrigin enumValue)
        {
            //StatusType enumValue = StatusType.UserNotOnProject;
            var displayNameAtt = enumValue.GetType().GetMember(enumValue.ToString())[0].GetCustomAttribute<DisplayAttribute>();
            return displayNameAtt == null ? enumValue.ToString() : displayNameAtt.Name;
        }
    }
}
