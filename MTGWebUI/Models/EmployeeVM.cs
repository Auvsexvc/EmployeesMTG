using MTGWebUI.Enums;
using System.ComponentModel.DataAnnotations;

namespace MTGWebUI.Models
{
    public class EmployeeVM : IDisposable
    {
        public Guid Id { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Street Name")]
        public string StreetName { get; set; } = string.Empty;

        [Display(Name = "House Number")]
        public string HouseNumber { get; set; } = string.Empty;

        [Display(Name = "Apartment Number")]
        public string? ApartmentNumber { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Display(Name = "Town")]
        public string Town { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Age")]
        public int Age { get; set; }

        public Operation State { get; set; }

        public void Dispose() => GC.Collect();
    }
}