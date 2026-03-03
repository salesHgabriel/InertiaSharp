namespace InertiaSharp.Shared.Contracts;

public record LoginRequest(string Email, string Password, bool Remember = false);