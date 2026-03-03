namespace InertiaSharp.Shared.Contracts;

public record UpdateProfileRequest(string FirstName, string LastName, string Email, string? Bio, string? PhoneNumber);