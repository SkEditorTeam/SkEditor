using System;
using System.IO;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Session.Models;

namespace SkEditor.Workspace.Tabs;

public class FileTabFactory(
    AppSettings appSettings,
    ExperimentsConfig experiments) : IFileTabFactory
{
    public FileTabViewModel CreateFromFile(BaseFile file)
    {
        if (file.Name is null)
        {
            throw new ArgumentException("File name cannot be null in this method");
        }

        TranslatableString header = TranslatableString.FromRawString(file.Name);

        return new FileTabViewModel(header, appSettings, experiments)
        {
            File = file
        };
    }

    public FileTabViewModel CreateFromSession(SessionStateFile file)
    {
        TranslatableString? header;
        if (file.Header is { } fileHeader)
        {
            header = fileHeader.ToTranslatable();
        }
        else if (file.Path is not null)
        {
            string fileName = Path.GetFileName(file.Path);
            header = fileName.ToTranslatable();
        }
        else
        {
            throw new InvalidOperationException("The session state file is missing both header and path!");
        }

        return new FileTabViewModel(header, appSettings, experiments)
        {
            File = new LocalFile(file.Path, file.Header),
            IsSaved = file.IsSaved
        };
    }

    public FileTabViewModel CreateNewFile(string name)
    {
        TranslatableString header = TranslatableString.FromRawString(name);

        return new FileTabViewModel(header, appSettings, experiments)
        {
            File = new LocalFile(name: name),
            IsSaved = true
        };
    }
}