using Microsoft.AspNetCore.Mvc;
using MTGWebApi.Interfaces;
using MTGWebApi.Models;

namespace MTGWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeesService;

        public EmployeeController(IEmployeeService employeesService)
        {
            _employeesService = employeesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string order = "asc", string searchString = "null", int pageNumber = 1, int pageSize = 0)
        {
            var employeesVMs = await _employeesService.GetAllAsync(order, searchString, pageNumber, pageSize);
            try
            {
                return Ok(employeesVMs);
            }
            catch (Exception)
            {
                return BadRequest("Employees couldnt be loaded.");
            }
        }

        [HttpGet("get-changes")]
        public async Task<IActionResult> GetChanges()
        {
            var employeesVMs = await _employeesService.GetPendingChangesAsync();

            if (!employeesVMs.Any())
            {
                return NoContent();
            }

            return Ok(employeesVMs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var employeeVM = await _employeesService.GetByIdAsync(id);

            return Ok(employeeVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            var id = await _employeesService.CreateAsync(dto);

            return Created($"/api/employee/{id}", null);
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save()
        {
            await _employeesService.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel()
        {
            await _employeesService.CancelChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateEmployeeDto dto)
        {
            await _employeesService.UpdateAsync(id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await _employeesService.DeleteAsync(id);

            return NoContent();
        }
    }
}