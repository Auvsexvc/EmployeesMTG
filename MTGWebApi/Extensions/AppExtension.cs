using MTGWebApi.Entities;
using MTGWebApi.Enums;
using MTGWebApi.Models;

namespace MTGWebApi.Extensions
{
    public static class AppExtension
    {
        public static EmployeeVM ConvertToEmployeeVM(this Employee employee)
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

        public static Employee ConvertToEmployee(this string line)
        {
            var values = line.Split(';');

            return new Employee()
            {
                Id = Guid.Parse(values[0]),
                FirstName = values[1],
                LastName = values[2],
                StreetName = values[3],
                HouseNumber = values[4],
                ApartmentNumber = values[5],
                PostalCode = values[6],
                Town = values[7],
                PhoneNumber = values[8],
                DateOfBirth = DateTime.Parse(values[9]),
                State = Enum.Parse<Operation>(values[10])
            };
        }
    }
}
