using MTGWebUI.Helper;
using MTGWebUI.Models;

namespace MTGWebUI.Extensions
{
    public static class AppExtension
    {
        public static IEnumerable<EmployeeVM> SortEmployeesByPropertyName(this IEnumerable<EmployeeVM> employees, string sortingOrder)
        {
            var propName = string.Concat(sortingOrder.TakeLast(4)) == "Desc" && sortingOrder.Length > 4 ? string.Concat(sortingOrder.SkipLast(4)) : sortingOrder;
            var propInfo = AppStatic.GetDisplayPropertiesForEmployeeVM().FirstOrDefault(p => p.Name == propName);

            if (propInfo == null)
            {
                return employees;
            }

            if (string.Concat(sortingOrder.TakeLast(4)) == "Desc" && sortingOrder.Length > 4)
            {
                return employees.OrderByDescending(e => propInfo.GetValue(e));
            }

            return employees.OrderBy(e => propInfo.GetValue(e));
        }
    }
}
