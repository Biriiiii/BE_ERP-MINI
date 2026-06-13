namespace BE_ERP_MINI.Contracts;

public sealed record LoginRequest(string Username, string Password);
public sealed record LoginResponse(string Token, string FullName, string Role);
