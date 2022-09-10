using System.ComponentModel.DataAnnotations;

namespace MTGWebUI.Enums
{
    public enum Operation
    {
        [Display(Name = "Persistent")]
        Persist,
        [Display(Name = "New record")]
        Create,
        [Display(Name = "Deleted record")]
        Delete,
        [Display(Name = "Updated record")]
        Update
    }
}