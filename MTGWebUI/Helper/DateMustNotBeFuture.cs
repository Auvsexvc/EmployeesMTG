using System.ComponentModel.DataAnnotations;

namespace MTGWebUI.Helper
{
    [AttributeUsageAttribute(AttributeTargets.All, AllowMultiple = false)]
    public class DateMustNotBeFutureAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value != null)
            {
                DateTime valueAsDate = (DateTime)value;
                if (DateTime.Compare(valueAsDate, DateTime.Today) > 0)
                {
                    ErrorMessage = "Date of birth cannot be in future from now";

                    return false;
                }
            }
            return true;
        }
    }
}