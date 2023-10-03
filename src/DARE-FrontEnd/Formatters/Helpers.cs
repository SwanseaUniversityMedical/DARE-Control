using BL.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

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
    }
}
