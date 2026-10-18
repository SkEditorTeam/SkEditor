using System;
using System.Collections.Generic;

namespace SkEditor.Analyzing.Models;

public record AnalyzerResult(List<AnalyzerError> Errors)
{
    public static AnalyzerResult FromDto(AnalyzeResultDto dto)
    {
        List<AnalyzerError> errors = [];

        foreach (ScriptErrorDto errorDto in dto.Errors)
        {
            AnalyzerError error = new(
                errorDto.Level switch
                {
                    ScriptErrorDto.AnalyzerErrorLevel.Severe => AnalyzerErrorType.Error,
                    ScriptErrorDto.AnalyzerErrorLevel.Warning => AnalyzerErrorType.Warning,
                    ScriptErrorDto.AnalyzerErrorLevel.Info => AnalyzerErrorType.Info,
                    _ => throw new ArgumentOutOfRangeException()
                },
                errorDto.Message,
                errorDto.Line
            );

            errors.Add(error);
        }

        return new AnalyzerResult(errors);
    }
}