namespace SkEditor.Workspace.Remote.Models;

public record RemoteConnectionTestResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static RemoteConnectionTestResult Error(string message)
    {
        return new RemoteConnectionTestResult
        {
            IsSuccess = false,
            Message = message
        };
    }

    public static RemoteConnectionTestResult Success(string message)
    {
        return new RemoteConnectionTestResult
        {
            IsSuccess = true,
            Message = message
        };
    }
}