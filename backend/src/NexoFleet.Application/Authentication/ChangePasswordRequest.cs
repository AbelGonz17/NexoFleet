namespace NexoFleet.Application.Authentication;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
