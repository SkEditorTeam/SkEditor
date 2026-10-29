namespace SkEditor.Analyzing;

public interface IAnalyzerInitializer
{
    /// <summary>
    ///     Observes whether the Analyzer is enabled and starts or stops the Analyzer accordingly.
    /// </summary>
    void ObserveOptionChange();
}