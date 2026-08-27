namespace RoleBasedRecords.Application.Common;

public enum AppError
{
    InvalidCredentials,
    Forbidden,
    NotFound
}

public sealed class AppException(AppError error, string message) : Exception(message)
{
    public AppError Error { get; } = error;
}
