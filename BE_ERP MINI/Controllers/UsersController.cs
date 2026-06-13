using BE_ERP_MINI.Contracts;
using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(AppDbContext db, ErpContext context, UserActionLogService actionLog) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? keyword, [FromQuery] Role? role, [FromQuery] bool? isActive)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.ACCOUNTANT);

        var query = db.Users.AsNoTracking().Where(x => !x.IsDeleted).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Username.Contains(keyword) || x.FullName.Contains(keyword) || x.Email.Contains(keyword));
        }
        if (role.HasValue) query = query.Where(x => x.Role == role.Value);
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

        return Ok(await query.OrderBy(x => x.Username).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.ACCOUNTANT);
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return user is null ? NotFound("Khong tim thay nguoi dung.") : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        if (await db.Users.AnyAsync(x => x.Username == request.Username && !x.IsDeleted))
        {
            return BadRequest("Username da ton tai.");
        }
        if (await db.Users.AnyAsync(x => x.Email == request.Email && !x.IsDeleted))
        {
            return BadRequest("Email da ton tai.");
        }

        var user = new AppUser
        {
            Username = request.Username.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
            IsActive = request.IsActive,
            CreatedBy = context.Current(Request).UserId ?? 0
        };
        await using var tx = await db.Database.BeginTransactionAsync();
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await actionLog.WriteAsync(Request, "CREATE", "User", user.Id.ToString(), $"Tao nguoi dung {user.Username}", null, user, user.Username);
        await tx.CommitAsync();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (user is null) return NotFound("Khong tim thay nguoi dung.");
        if (request.Version.HasValue && request.Version.Value != user.Version)
        {
            return UnprocessableEntity("Du lieu da bi thay doi boi nguoi khac. Vui long tai lai va thu lai.");
        }
        var oldValue = new { user.Id, user.Username, user.FullName, user.Email, user.PhoneNumber, user.Role, user.IsActive };

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email && await db.Users.AnyAsync(x => x.Email == request.Email && !x.IsDeleted))
        {
            return BadRequest("Email da ton tai.");
        }

        user.FullName = request.FullName?.Trim() ?? user.FullName;
        user.Email = request.Email?.Trim() ?? user.Email;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.Role = request.Role ?? user.Role;
        user.IsActive = request.IsActive ?? user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = context.Current(Request).UserId;
        user.Version++;

        await using var tx = await db.Database.BeginTransactionAsync();
        await db.SaveChangesAsync();
        await actionLog.WriteAsync(Request, "UPDATE", "User", user.Id.ToString(), $"Sua nguoi dung {user.Username}", oldValue, user, user.Username, request.Reason);
        await tx.CommitAsync();
        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, [FromQuery] string reason, [FromQuery] int? version)
    {
        context.Require(Request, Role.OWNER);
        if (string.IsNullOrWhiteSpace(reason)) return BadRequest("Xoa mem bat buoc nhap ly do.");
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (user is null) return NotFound("Khong tim thay nguoi dung.");
        if (version.HasValue && version.Value != user.Version)
        {
            return UnprocessableEntity("Du lieu da bi thay doi boi nguoi khac. Vui long tai lai va thu lai.");
        }
        var oldValue = new { user.Id, user.Username, user.FullName, user.Email, user.PhoneNumber, user.Role, user.IsActive };

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = context.Current(Request).UserId;
        user.IsActive = false;
        user.Version++;

        await using var tx = await db.Database.BeginTransactionAsync();
        await db.SaveChangesAsync();
        await actionLog.WriteAsync(Request, "SOFT_DELETE", "User", id.ToString(), $"Xoa mem nguoi dung {user.Username}", oldValue, null, user.Username, reason);
        await tx.CommitAsync();
        return NoContent();
    }
}
