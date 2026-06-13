using BE_ERP_MINI.Contracts;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_ERP_MINI.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await authService.LoginAsync(request.Username, request.Password);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
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
