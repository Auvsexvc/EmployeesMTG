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

        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetEmployeesAsync();
            var pendingChanges = await _employeeService.GetPendingChanges();

            if (!pendingChanges.Any())
            {
                ViewBag.Pending = "disabled";
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

        public async Task<IActionResult> Pending()
        {
            var employeesPending = await _employeeService.GetPendingChanges();
            if(!employeesPending.Any())
            {
                return RedirectToAction("Index");
            }

            return View(employeesPending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save()
        {
            await _employeeService.Save();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel()
        {
            await _employeeService.Cancel();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Filter(string searchString)
        {
            var employees = await _employeeService.GetEmployeesAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                var filteredResult = employees.Where(e => e.GetType().GetProperties().Select(p => p.GetValue(e)!.ToString()!.ToLower()).Any(p => p.Contains(searchString.ToLower())));
                ViewBag.SearchString = searchString;

                return View("Index", filteredResult);
            }

            return View("Index", employees);
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
    }
}