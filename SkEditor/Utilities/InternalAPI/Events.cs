using System;

namespace SkEditor.API;

public class Events : IEvents
{
    
    public event EventHandler OnPostEnable;
    public void PostEnable() => OnPostEnable.Invoke(this, EventArgs.Empty);
    
}