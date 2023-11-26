using Avalonia.Input;
using System.Runtime.InteropServices;

namespace SkEditor.Utilities;
public class KeyUtility
{
	public static KeyModifiers GetControlModifier()
	{
		return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? KeyModifiers.Meta : KeyModifiers.Control;
	}
}
