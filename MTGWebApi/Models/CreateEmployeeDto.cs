using System.ComponentModel.DataAnnotations;

namespace MTGWebApi.Models
{
    public class CreateEmployeeDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string StreetName { get; set; } = string.Empty;

        [Required]
        public string HouseNumber { get; set; } = string.Empty;

        public string? ApartmentNumber { get; set; }

        [Required]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        public string Town { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }
    }
}