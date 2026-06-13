using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BE_ERP_MINI.Contracts;
using BE_ERP_MINI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BE_ERP_MINI.Services;

public sealed class AuthService(AppDbContext db, IConfiguration configuration)
{
    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Username == username && !x.IsDeleted);
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Tai khoan khong ton tai hoac bi khoa.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Mat khau khong chinh xac.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var keyStr = configuration["JwtSettings:Secret"] ?? "BE_ERP_MINI_SUPER_SECRET_KEY_1234567890";
        var key = Encoding.ASCII.GetBytes(keyStr);
        var expiryMinutes = configuration.GetValue<int>("JwtSettings:ExpiryMinutes", 1440);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = configuration["JwtSettings:Issuer"],
            Audience = configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new LoginResponse(tokenString, user.FullName, user.Role.ToString());
    }
}
