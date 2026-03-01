namespace SkEditor.Models.Remote;

public record RemoteConnectionTestResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static RemoteConnectionTestResult Error(string message) => new()
    {
        IsSuccess = false,
        Message = message,
    };

    public static RemoteConnectionTestResult Success(string message) => new()
    {
        IsSuccess = true,
        Message = message,
    };
}