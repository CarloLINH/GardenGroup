// Author: Mart
using System.Security.Claims;
using GardenGroup.Authorization;

namespace GardenGroup.Extensions;

//Reads the logged-in employee from the claims that AccountController.Login puts in the cookie.
public static class UserClaimsExtensions
{
    //The employeeId of the logged-in user, or null when it is missing or not a number.
    public static int? GetEmployeeId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    //True when the logged-in user has a Service Desk role.
    public static bool IsServiceDesk(this ClaimsPrincipal user) =>
        AppRoles.IsServiceDesk(user.FindFirstValue(ClaimTypes.Role));
}