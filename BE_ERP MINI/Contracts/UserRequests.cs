using BE_ERP_MINI.Domain;

namespace BE_ERP_MINI.Contracts;

public sealed record CreateUserRequest(string Username, string FullName, string Email, string? PhoneNumber, Role Role, bool IsActive);
public sealed record UpdateUserRequest(string? FullName, string? Email, string? PhoneNumber, Role? Role, bool? IsActive, int? Version, string? Reason);
