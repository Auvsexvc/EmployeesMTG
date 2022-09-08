using MTGWebApi.Data;
using MTGWebApi.Entities;
using MTGWebApi.Exceptions;
using MTGWebApi.Helper;
using MTGWebApi.Interfaces;
using MTGWebApi.Models;
using MTGWebApi.Enums;

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

            await _appDbContext.AddAsync(employee);

            _logger.LogInformation(string.Format(Messages.MSG_CREATED, employee.Id));

            return employee.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            _logger.LogInformation(string.Format(Messages.MSG_DELETEINVOKED, id));

            var employees = await _appDbContext.GetEmployeesAsync();
            var employee = employees.Find(x => x.Id == id);

            if (employee is null)
            {
                throw new NotFoundException(string.Format(Messages.MSG_NOTFOUND, id));
            }
            employee.State = Operation.Delete;

            await _appDbContext.DeleteAsync(employee);

            _logger.LogInformation(string.Format(Messages.MSG_DELETED, id));
        }

        public async Task<IEnumerable<EmployeeVM>> GetAllAsync()
        {
            var employees = await _appDbContext.GetEmployeesAsync();

            List<EmployeeVM> employeeVMs = new();

            foreach (var employee in employees)
            {
                EmployeeVM employeeVM = MakeEmployeeVM(employee);
                employeeVMs.Add(employeeVM);
            }

            return employeeVMs;
        }

        public async Task<EmployeeVM> GetByIdAsync(Guid id)
        {
            var employees = await _appDbContext.GetEmployeesAsync();
            var employee = employees.Find(x => x.Id == id);

            if (employee is null)
            {
                throw new NotFoundException(string.Format(Messages.MSG_NOTFOUND, id));
            }

            return MakeEmployeeVM(employee);
        }

        public async Task UpdateAsync(Guid id, UpdateEmployeeDto dto)
        {
            _logger.LogInformation(string.Format(Messages.MSG_PUTINVOKED, id));

            var employees = await _appDbContext.GetEmployeesAsync();
            var employee = employees.Find(x => x.Id == id);

            if (employee is null)
            {
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

            await _appDbContext.UpdateAsync(employee);

            _logger.LogInformation(string.Format(Messages.MSG_UPDATED, id));
        }

        public async Task SaveChangesAsync()
        {
            var pendingChanges = await _appDbContext.PendingChangesAsync();
            if (pendingChanges.Any())
            {
                await _appDbContext.SaveChangesAsync();
            }
            _logger.LogInformation(string.Format(Messages.MSG_SAVED, pendingChanges.Count()));
        }

        public async Task<IEnumerable<EmployeeVM>> GetPendingChangesAsync()
        {
            var pendingChanges = await _appDbContext.PendingChangesAsync();

            return pendingChanges.Select(x=>MakeEmployeeVM(x));
        }

        public async Task CancelChangesAsync()
        {
            var pendingChanges = await _appDbContext.PendingChangesAsync();
            await _appDbContext.CancelChangesAsync();
            _logger.LogInformation(string.Format(Messages.MSG_CANCELED, pendingChanges.Count()));
        }

        private static EmployeeVM MakeEmployeeVM(Employee employee)
        {
            return new()
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                StreetName = employee.StreetName,
                HouseNumber = employee.HouseNumber,
                ApartmentNumber = employee.ApartmentNumber,
                PostalCode = employee.PostalCode,
                Town = employee.Town,
                PhoneNumber = employee.PhoneNumber,
                DateOfBirth = employee.DateOfBirth,
                State = employee.State
            };
        }
    }
}