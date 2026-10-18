using System.Collections.Generic;

namespace SkEditor.Utilities.Editor.TextMarkers;

public interface ITextMarkerService
{
    IEnumerable<ITextMarker> TextMarkers { get; }
    ITextMarker? Create(int startOffset, int length);

    void Remove(ITextMarker marker);

    void RemoveAll();

    IEnumerable<ITextMarker> GetMarkersAtOffset(int offset);
}