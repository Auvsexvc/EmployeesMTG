using Microsoft.AspNetCore.Mvc;
using MTGWebUI.Interfaces;
using MTGWebUI.Models;

namespace MTGWebUI.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index(string? sortingOrder, string? searchString = "")
        {
            sortingOrder = SessionHandlerForSorting(sortingOrder);
            searchString = SessionHandlerForSearching(searchString);

            var employees = await _employeeService.GetEmployeesAsync();
            var areTherePendingChanges = await _employeeService.AreTherePendingChanges();

            if (!areTherePendingChanges)
            {
                ViewBag.Pending = "disabled";
            }


            foreach (var item in new EmployeeVM().GetType().GetProperties().Where(p => p.Name != "Id" && p.Name != "State").Select(p => p.Name))
            {
                ViewData[item] = sortingOrder == item ? item + "Desc" : item;
            }

            employees = SortEmployeesByPropertyName(employees, sortingOrder);


            if (!string.IsNullOrEmpty(searchString))
            {
                var filteredResult = employees.Where(e => e.GetType().GetProperties().Where(p => p.Name != "Id" && p.Name != "State").Select(p => p.GetValue(e)!.ToString()!.ToLower()).Any(p => p.Contains(searchString.ToLower())));
                ViewBag.SearchString = searchString;

                return View(filteredResult);
            }

            return View(employees);
        }

        public async Task<IActionResult> Details(Guid id) => await GetEmployeeAsync(id);

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeDto createEmployeeDto)
        {
            if (!ModelState.IsValid)
            {
                return View(createEmployeeDto);
            }
            await _employeeService.AddEmployeeAsync(createEmployeeDto);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(Guid id) => await GetEmployeeAsync(id);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            if (!ModelState.IsValid)
            {
                return View(updateEmployeeDto);
            }
            await _employeeService.EditEmployeeAsync(id, updateEmployeeDto);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(Guid id) => await GetEmployeeAsync(id);

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                return View("NotFound");
            }
            await _employeeService.DeleteAsync(id);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Pending(string? sortingOrder, string? searchString = "")
        {

            var employeesPending = await _employeeService.GetPendingChangesAsync();
            if (!employeesPending.Any())
            {
                return RedirectToAction("Index");
            }

            sortingOrder = SessionHandlerForSorting(sortingOrder);
            searchString = SessionHandlerForSearching(searchString);

            foreach (var item in new EmployeeVM().GetType().GetProperties().Where(p => p.Name != "Id" && p.Name != "State").Select(p => p.Name))
            {
                ViewData[item] = sortingOrder == item ? item + "Desc" : item;
            }

            employeesPending = SortEmployeesByPropertyName(employeesPending, sortingOrder);

            if (!string.IsNullOrEmpty(searchString))
            {
                var filteredResult = employeesPending.Where(e => e.GetType().GetProperties().Where(p => p.Name != "Id" && p.Name != "State").Select(p => p.GetValue(e)!.ToString()!.ToLower()).Any(p => p.Contains(searchString.ToLower())));
                ViewBag.SearchString = searchString;

                return View(filteredResult);
            }

            return View(employeesPending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save()
        {
            await _employeeService.SaveAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel()
        {
            await _employeeService.CancelAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Filter(string searchString)
        {
            var employees = await _employeeService.GetEmployeesAsync();
            var pendingChanges = await _employeeService.GetPendingChangesAsync();

            if (!pendingChanges.Any())
            {
                ViewBag.Pending = "disabled";
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                var filteredResult = employees.Where(e => e.GetType().GetProperties().Select(p => p.GetValue(e)!.ToString()!.ToLower()).Any(p => p.Contains(searchString.ToLower())));
                ViewBag.SearchString = searchString;

                return View("Index", filteredResult);
            }

            return View("Index", employees);
        }

        private string? SessionHandlerForSearching(string? searchString)
        {
            if (searchString == null)
            {
                HttpContext.Session.SetString("searchString", "");
                searchString = "";
            }
            else if (searchString != "")
            {
                HttpContext.Session.SetString("searchString", searchString);
            }
            else
            {
                if (!string.IsNullOrEmpty(HttpContext.Session.GetString("searchString")))
                {
                    searchString = HttpContext.Session.GetString("searchString");
                }
                else
                {
                    HttpContext.Session.SetString("searchString", String.Empty);
                    searchString = String.Empty;
                }
            }

            return searchString;
        }

        private string SessionHandlerForSorting(string? sortingOrder)
        {
            if (sortingOrder != null)
            {
                HttpContext.Session.SetString("sortingOrder", sortingOrder);
            }
            else
            {
                sortingOrder = HttpContext.Session.GetString("sortingOrder") ?? "FirstName";
            }

            return sortingOrder;
        }

        private async Task<IActionResult> GetEmployeeAsync(Guid id)
        {
            if (Request.Headers["Referer"] != string.Empty)
            {
                ViewData["Reffer"] = Request.Headers["Referer"].ToString();
            }

            var employeeDetails = await _employeeService.GetEmployeeByIdAsync(id);

            if (employeeDetails == null)
            {
                return View("NotFound");
            }

            return View(employeeDetails);
        }

        private IEnumerable<EmployeeVM> SortEmployeesByPropertyName(IEnumerable<EmployeeVM> employees, string propName)
        {
            employees = propName switch
            {
                "FirstName" => employees.OrderBy(e => e.FirstName),
                "FirstNameDesc" => employees.OrderByDescending(e => e.FirstName),
                "LastName" => employees.OrderBy(e => e.LastName),
                "LastNameDesc" => employees.OrderByDescending(e => e.LastName),
                "StreetName" => employees.OrderBy(e => e.StreetName),
                "StreetNameDesc" => employees.OrderByDescending(e => e.StreetName),
                "HouseNumber" => employees.OrderBy(e => e.HouseNumber),
                "HouseNumberDesc" => employees.OrderByDescending(e => e.HouseNumber),
                "ApartmentNumber" => employees.OrderBy(e => e.ApartmentNumber),
                "ApartmentNumberDesc" => employees.OrderByDescending(e => e.ApartmentNumber),
                "PostalCode" => employees.OrderBy(e => e.PostalCode),
                "PostalCodeDesc" => employees.OrderByDescending(e => e.PostalCode),
                "Town" => employees.OrderBy(e => e.Town),
                "TownDesc" => employees.OrderByDescending(e => e.Town),
                "PhoneNumber" => employees.OrderBy(e => e.PhoneNumber),
                "PhoneNumberDesc" => employees.OrderByDescending(e => e.PhoneNumber),
                "DateOfBirth" => employees.OrderBy(e => e.DateOfBirth),
                "DateOfBirthDesc" => employees.OrderByDescending(e => e.DateOfBirth),
                "Age" => employees.OrderBy(e => e.Age),
                "AgeDesc" => employees.OrderByDescending(e => e.Age),
                _ => employees.OrderBy(e => e.FirstName),
            };

            return employees;
        }
    }
}