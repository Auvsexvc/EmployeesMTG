using MTGWebUI.Models;
using System.Reflection;

namespace MTGWebUI.Helper
{
    public static class AppStatic
    {
        public static IEnumerable<PropertyInfo> GetDisplayPropertiesForEmployeeVM()
        {
            using var obj = new EmployeeVM();

            return obj.GetType().GetProperties().Where(p => p.Name != "Id" && p.Name != "State");
        }
    }
}
