using MTGWebApi.Data;
using MTGWebApi.Entities;
using MTGWebApi.Enums;
using MTGWebApi.Exceptions;
using MTGWebApi.Extensions;
using MTGWebApi.Helper;
using MTGWebApi.Interfaces;
using MTGWebApi.Models;

namespace MTGWebApi.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IAppDbContext _appDbContext;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(IAppDbContext appDbContext, ILogger<EmployeeService> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task<Guid> CreateAsync(CreateEmployeeDto dto)
        {
            var employee = new Employee()
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                StreetName = dto.StreetName,
                HouseNumber = dto.HouseNumber,
                ApartmentNumber = dto.ApartmentNumber,
                PostalCode = dto.PostalCode,
                Town = dto.Town,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                State = Operation.Create
            };

            await _appDbContext.StageAddAsync(employee);

            _logger.LogInformation(string.Format(Messages.MSG_CREATESTAGED, employee.Id));

            return employee.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            _logger.LogInformation(string.Format(Messages.MSG_DELETEINVOKED, id));

            var employees = await _appDbContext.GetEmployeesAsync();
            var employee = employees.FirstOrDefault(x => x.Id == id);

            if (employee is null)
            {
                _logger.LogInformation(string.Format(Messages.MSG_NOTFOUND, id));
                throw new NotFoundException(string.Format(Messages.MSG_NOTFOUND, id));
            }
            employee.State = Operation.Delete;

            await _appDbContext.StageDeleteAsync(employee);

            _logger.LogInformation(string.Format(Messages.MSG_DELETESTAGED, id));
        }

        public async Task<IEnumerable<EmployeeVM>> GetAllAsync(string order, string searchString, int? pageNumber, int pageSize)
        {
            var employees = await _appDbContext.GetEmployeesAsync();
            if (!string.IsNullOrEmpty(searchString) && searchString != "null")
            {
                employees = employees.Where(e => e.GetType().GetProperties().Select(p => p.GetValue(e)!.ToString()!.ToLower()).Any(p => p.Contains(searchString.ToLower())));
            }

            if (!string.IsNullOrEmpty(order))
            {
                employees = order switch
                {
                    "desc" => employees.OrderByDescending(p => p.LastName),
                    _ => employees.OrderBy(p => p.LastName)
                };
            }

            List<EmployeeVM> employeeVMs = new();

            foreach (var employee in employees)
            {
                EmployeeVM employeeVM = employee.ConvertToEmployeeVM();
                employeeVMs.Add(employeeVM);
            }

            return PaginetedList<EmployeeVM>.Create(employeeVMs.AsQueryable(), pageNumber ?? 1, pageSize == 0 ? employeeVMs.Count : pageSize);
        }

        public async Task<EmployeeVM> GetByIdAsync(Guid id)
        {
            var employees = await _appDbContext.GetEmployeesAsync();
            var employee = employees.FirstOrDefault(x => x.Id == id);

            if (employee is null)
            {
                _logger.LogInformation(string.Format(Messages.MSG_NOTFOUND, id));
                throw new NotFoundException(string.Format(Messages.MSG_NOTFOUND, id));
            }

            return employee.ConvertToEmployeeVM();
        }

        public async Task UpdateAsync(Guid id, UpdateEmployeeDto dto)
        {
            _logger.LogInformation(string.Format(Messages.MSG_PUTINVOKED, id));

            var employees = await _appDbContext.GetEmployeesAsync();
            var employee = employees.FirstOrDefault(x => x.Id == id);

            if (employee is null)
            {
                _logger.LogInformation(string.Format(Messages.MSG_NOTFOUND, id));
                throw new NotFoundException(string.Format(Messages.MSG_NOTFOUND, id));
            }

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.StreetName = dto.StreetName;
            employee.HouseNumber = dto.HouseNumber;
            employee.ApartmentNumber = dto.ApartmentNumber;
            employee.PostalCode = dto.PostalCode;
            employee.Town = dto.Town;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.DateOfBirth = dto.DateOfBirth;
            employee.State = Operation.Update;

            await _appDbContext.StageUpdateAsync(employee);

            _logger.LogInformation(string.Format(Messages.MSG_UPDATESTAGED, id));
        }

        public async Task SaveChangesAsync()
        {
            var pendingChanges = await _appDbContext.GetStagedChangesAsync();

            if (pendingChanges.Any())
            {
                await _appDbContext.CommitChangesAsync();
            }
            _logger.LogInformation(string.Format(Messages.MSG_SAVED, pendingChanges.Count()));
        }

        public async Task<IEnumerable<EmployeeVM>> GetPendingChangesAsync()
        {
            var pendingChanges = await _appDbContext.GetStagedChangesAsync();

            return pendingChanges.Select(x => x.ConvertToEmployeeVM());
        }

        public async Task CancelChangesAsync()
        {
            var pendingChanges = await _appDbContext.GetStagedChangesAsync();
            await _appDbContext.CancelChangesAsync();
            _logger.LogInformation(string.Format(Messages.MSG_CANCELED, pendingChanges.Count()));
        }
    }
}