// Author: Mart
namespace GardenGroup.Authorization
{
    public static class AppRoles
    {
        public const string Employee = "employee";
        public const string Agent = "agent";
        public const string Manager = "manager";

        public const string Admin = "admin";

        //Comma separated for [Authorize(Roles = AppRoles.ServiceDeskRoles)]
        public const string ServiceDeskRoles = Agent + "," + Manager +","+ Admin;

        //True when the role belongs to the Service Desk (case-insensitive).
        public static bool IsServiceDesk(string? role) =>
            string.Equals(role, Agent, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, Manager, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase);
    }
}
