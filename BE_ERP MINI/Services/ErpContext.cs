using System.Security.Claims;
using BE_ERP_MINI.Domain;

namespace BE_ERP_MINI.Services;

public sealed class ErpContext
{
    public (int? UserId, Role Role) Current(HttpRequest request)
    {
        var user = request.HttpContext.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleStr = user.FindFirstValue(ClaimTypes.Role) ?? Role.OWNER.ToString();
            Enum.TryParse<Role>(roleStr, true, out var role);
            return (int.TryParse(userIdStr, out var userId) ? userId : null, role);
        }

        // Fallback for missing auth / swagger testing without token if not enforced
        return (null, Role.EMPLOYEE);
    }

    public void Require(HttpRequest request, params Role[] roles)
    {
        var current = Current(request);
        if (!roles.Contains(current.Role))
            throw new UnauthorizedAccessException($"Ban khong co quyen thuc hien thao tac nay. (Role: {current.Role})");
    }
}
