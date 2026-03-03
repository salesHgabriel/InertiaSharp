
namespace InertiaSharp.Shared.Contracts;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string NewPasswordConfirmation);