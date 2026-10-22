using GardenGroup.Enums;

namespace GardenGroup.Extensions
{
    public static class TicketUIExtension
    {
        public static string GetBadgeColor(this Statuses status)
        {
            return status switch
            {
                Statuses.Open => "bg-success",
                Statuses.Closed => "bg-secondary",
                _ => "bg-warning text-dark"
            };
        }

        public static string GetPriorityClass(this Priorities priority)
        {
            return priority switch
            {
                Priorities.Urgent => "bg-dark", 
                Priorities.High => "bg-danger",
                Priorities.Medium => "bg-warning text-dark",
                _ => "bg-info text-dark"
            };
        }
    }
}
