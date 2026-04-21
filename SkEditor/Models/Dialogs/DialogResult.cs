namespace SkEditor.Models.Dialogs;

public class DialogResult(StandardDialogResult result)
{
    public StandardDialogResult Result { get; } = result;
    public string? Data { get; init; } = null;

    public static DialogResult Ok => new(StandardDialogResult.Ok);
    public static DialogResult Cancel => new(StandardDialogResult.Cancel);
    public static DialogResult Yes => new(StandardDialogResult.Yes);
    public static DialogResult No => new(StandardDialogResult.No);

    public override string ToString()
    {
        if (Result == StandardDialogResult.Custom)
        {
            return Data ?? "Custom";
        }

        return Result.ToString();
    }

    public static bool operator ==(DialogResult left, StandardDialogResult right)
    {
        return left.Result == right;
    }

    public static bool operator !=(DialogResult left, StandardDialogResult right)
    {
        return left.Result != right;
    }

    public override bool Equals(object? obj)
    {
        if (obj is DialogResult dialogResult)
        {
            return Result == dialogResult.Result;
        }

        return false;
    }

    public override int GetHashCode()
    {
        if (Result == StandardDialogResult.Custom)
        {
            return Data?.GetHashCode() ?? 0;
        }

        return Result.GetHashCode();
    }
}

public enum StandardDialogResult
{
    Yes,
    No,
    Ok,
    Cancel,
    Custom
}