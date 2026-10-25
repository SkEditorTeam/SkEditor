using System;
using System.Threading.Tasks;

namespace SkEditor.Windowing.Errors;

public interface ICrashService
{
    void SaveCrashState(Exception ex);
    Task CheckForCrashes();
}