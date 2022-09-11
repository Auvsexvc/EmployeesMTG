using MTGWebUI.Exceptions;
using MTGWebUI.Interfaces;
using MTGWebUI.Models;

namespace MTGWebUI.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IConfiguration _configuration;

        public EmployeeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IEnumerable<EmployeeVM>> GetEmployeesAsync()
        {
            IEnumerable<EmployeeVM>? employees = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.GetConnectionString("DefaultConnection"));
                var result = await client.GetAsync("Employee?order=asc&searchString=null&pageNumber=1&pageSize=0");

                if (result.IsSuccessStatusCode)
                {
                    employees = await result.Content.ReadFromJsonAsync<IList<EmployeeVM>>();
                }
                else
                {
                    throw new BadRequestException();
                }
            }

            return employees!;
        }

        public async Task<EmployeeVM> GetEmployeeByIdAsync(Guid id)
        {
            EmployeeVM? employee = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.GetConnectionString("DefaultConnection"));
                var result = await client.GetAsync($"Employee/{id}");

                if (result.IsSuccessStatusCode)
                {
                    employee = await result.Content.ReadFromJsonAsync<EmployeeVM>();
                }
            }

            return employee!;
        }

        public async Task<HttpResponseMessage> AddEmployeeAsync(CreateEmployeeDto createEmployeeDto)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(_configuration.GetConnectionString("DefaultConnection"));

            var result = await client.PostAsJsonAsync<CreateEmployeeDto>("Employee", createEmployeeDto);

            return result;
        }

        public async Task<IEnumerable<EmployeeVM>> GetPendingChangesAsync()
        {
            IEnumerable<EmployeeVM>? employees = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration.GetConnectionString("DefaultConnection"));
                var result = await client.GetAsync("Employee/get-changes");

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    employees = await result.Content.ReadFromJsonAsync<IList<EmployeeVM>>();
                }
                else
                {
                    employees = Enumerable.Empty<EmployeeVM>();
                }
            }

            return employees!;
        }

        public async Task<HttpResponseMessage> EditEmployeeAsync(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(_configuration.GetConnectionString("DefaultConnection"));

            var result = await client.PutAsJsonAsync<UpdateEmployeeDto>($"Employee/{id}", updateEmployeeDto);

            return result;
        }

        public async Task<HttpResponseMessage> SaveAsync()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(_configuration.GetConnectionString("DefaultConnection"));

            var result = await client.PostAsync("Employee/save", null);

            return result;
        }

        public async Task<HttpResponseMessage> CancelAsync()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(_configuration.GetConnectionString("DefaultConnection"));

            var result = await client.PostAsync("Employee/cancel", null);

            return result;
        }

        public async Task<HttpResponseMessage> DeleteAsync(Guid id)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(_configuration.GetConnectionString("DefaultConnection"));

            var result = await client.DeleteAsync($"Employee/{id}");

            return result;
        }

        public async Task<bool> AreTherePendingChanges()
        {
            var pendingChanges = await GetPendingChangesAsync();

            return pendingChanges.Any();
        }
    }
}