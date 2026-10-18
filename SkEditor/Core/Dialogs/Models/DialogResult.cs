namespace SkEditor.Models.Dialogs;

/// <summary>
///     Represents the result of a dialog. This can be one of the standard dialog results (Yes, No, Ok, Cancel) or a custom
///     result with associated data.
/// </summary>
public class DialogResult
{
    /// <summary>
    ///     Gets a <see cref="DialogResult" /> representing the "Cancel" standard dialog result.
    /// </summary>
    public static DialogResult Cancel => new(StandardDialogResult.Cancel);

    /// <summary>
    ///     Gets the custom data associated with the dialog result. This will be non-null only if the result is a custom
    ///     result.
    /// </summary>
    public string? Data { get; }

    /// <summary>
    ///     Gets a <see cref="DialogResult" /> representing the "No" standard dialog result.
    /// </summary>
    public static DialogResult No => new(StandardDialogResult.No);

    /// <summary>
    ///     Gets a <see cref="DialogResult" /> representing the "Ok" standard dialog result.
    /// </summary>
    public static DialogResult Ok => new(StandardDialogResult.Ok);

    /// <summary>
    ///     Gets the standard dialog result. If the result is a custom result, this will be
    ///     <see cref="StandardDialogResult.Custom" />.
    /// </summary>
    public StandardDialogResult Result { get; }

    /// <summary>
    ///     Gets a <see cref="DialogResult" /> representing the "Yes" standard dialog result.
    /// </summary>
    public static DialogResult Yes => new(StandardDialogResult.Yes);

    /// <summary>
    ///     Initializes a new instance of the <see cref="DialogResult" /> class with a standard dialog result.
    /// </summary>
    /// <param name="result">The standard dialog result.</param>
    public DialogResult(StandardDialogResult result) => Result = result;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DialogResult" /> class with a custom dialog result and associated
    ///     data.
    /// </summary>
    /// <param name="customData">The custom data associated with the dialog result.</param>
    public DialogResult(string customData)
    {
        Result = StandardDialogResult.Custom;
        Data = customData;
    }

    public override string ToString()
    {
        if (Result == StandardDialogResult.Custom)
        {
            return Data ?? "Custom";
        }

        return Result.ToString();
    }

    /// <returns>
    ///     True if the <see cref="DialogResult" /> has the same standard result as the specified
    ///     <see cref="StandardDialogResult" />; otherwise, false.
    /// </returns>
    public static bool operator ==(DialogResult left, StandardDialogResult right)
    {
        return left.Result == right;
    }

    /// <returns>
    ///     True if the <see cref="DialogResult" /> has a different standard result than the specified
    ///     <see cref="StandardDialogResult" />; otherwise, false.
    /// </returns>
    public static bool operator !=(DialogResult left, StandardDialogResult right)
    {
        return left.Result != right;
    }

    /// <summary>
    ///     Compares two <see cref="DialogResult" /> instances for equality. If both results are custom, their associated data
    ///     will be compared for equality. Otherwise, their standard results will be compared.
    /// </summary>
    /// <returns>True if the two <see cref="DialogResult" /> instances are considered equal; otherwise, false.</returns>
    public static bool operator ==(DialogResult left, DialogResult right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        if (left.Result == StandardDialogResult.Custom && right.Result == StandardDialogResult.Custom)
        {
            return left.Data == right.Data;
        }

        return left.Result == right.Result;
    }

    /// <summary>
    ///     Compares two <see cref="DialogResult" /> instances for inequality. If both results are custom, their associated
    ///     data will be compared for inequality. Otherwise, their standard results will be compared.
    /// </summary>
    /// <returns>True if the two <see cref="DialogResult" /> instances are considered not equal; otherwise, false.</returns>
    public static bool operator !=(DialogResult left, DialogResult right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Compares the specified object with the current <see cref="DialogResult" /> for equality. This will include other
    ///     <see cref="DialogResult" /> instances as well as <see cref="StandardDialogResult" /> values.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="DialogResult" />.</param>
    /// <returns>True if the specified object is equal to the current <see cref="DialogResult" />; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            DialogResult other when Result == StandardDialogResult.Custom => Data == other.Data,
            DialogResult other => Result == other.Result,
            StandardDialogResult standardResult => Result == standardResult,
            _ => false
        };
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

/// <summary>
///     Represents the standard dialog results that can be returned by a dialog. These include "Yes", "No", "Ok", and
///     "Cancel". The "Custom" result can be used to represent a custom dialog result with associated data, which can be
///     accessed through the <see cref="DialogResult.Data" /> property.
/// </summary>
public enum StandardDialogResult
{
    Yes,
    No,
    Ok,
    Cancel,
    Custom
}