namespace InertiaSharp.Shared.Contracts;

public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string PasswordConfirmation);