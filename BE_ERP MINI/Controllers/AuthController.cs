using BE_ERP_MINI.Contracts;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_ERP_MINI.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService authService, UserActionLogService actionLog) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await authService.LoginAsync(request.Username, request.Password);
            // Ghi log LOGIN thành công
            await actionLog.AddLogAsync(Request, "LOGIN", "User", "0", $"User {request.Username} dang nhap thanh cong", null, null);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Ghi log LOGIN_FAILED
            await actionLog.AddLogAsync(Request, "LOGIN_FAILED", "User", "0", $"User {request.Username} dang nhap that bai: {ex.Message}", null, null);
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await actionLog.AddLogAsync(Request, "LOGOUT", "User", "0", "User dang xuat", null, null);
        return Ok(new { message = "Dang xuat thanh cong." });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me([FromServices] ErpContext context)
    {
        var current = context.Current(Request);
        if (current.UserId is null)
            return Unauthorized(new { error = "Chua dang nhap." });

        return Ok(new
        {
            UserId = current.UserId,
            Role = current.Role.ToString()
        });
    }
}
