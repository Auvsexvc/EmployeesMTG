using MTGWebApi.Enums;

namespace MTGWebApi.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string? ApartmentNumber { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public Operation State { get; set; }
    }
}