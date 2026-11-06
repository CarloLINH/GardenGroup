using System.ComponentModel.DataAnnotations;

namespace GardenGroup.Enums
{
    public enum Statuses
    {
        Open,
        [Display(Name = "In progress")]
        In_Progress,
        Closed,
        Resolved,
        Escalated
    }
}
