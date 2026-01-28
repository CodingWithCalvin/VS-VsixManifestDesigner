using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CodingWithCalvin.VsixManifestDesigner.Models;
using CodingWithCalvin.VsixManifestDesigner.Services;

namespace CodingWithCalvin.VsixManifestDesigner.ViewModels;

/// <summary>
/// ViewModel for the VSIX manifest editor.
/// </summary>
public sealed class ManifestViewModel : ViewModelBase
{
    private readonly ManifestService _manifestService;
    private VsixManifest? _manifest;
    private int _selectedTabIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestViewModel"/> class.
    /// </summary>
    /// <param name="manifestService">The manifest service.</param>
    public ManifestViewModel(ManifestService manifestService)
    {
        _manifestService = manifestService;
    }

    /// <summary>
    /// Gets or sets the manifest.
    /// </summary>
    public VsixManifest? Manifest
    {
        get => _manifest;
        private set => SetProperty(ref _manifest, value);
    }

    /// <summary>
    /// Gets or sets the path to the manifest file being edited.
    /// </summary>
    public string? ManifestFilePath { get; set; }

    /// <summary>
    /// Gets or sets the selected tab index.
    /// </summary>
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    #region Metadata Properties

    /// <summary>
    /// Gets or sets the extension ID.
    /// </summary>
    public string Id
    {
        get => Manifest?.Metadata.Identity.Id ?? string.Empty;
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.Identity.Id = value;
                OnPropertyChanged();
                ValidateId();
            }
        }
    }

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public string Version
    {
        get => Manifest?.Metadata.Identity.Version ?? "1.0.0";
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.Identity.Version = value;
                OnPropertyChanged();
                ValidateVersion();
            }
        }
    }

    /// <summary>
    /// Gets or sets the publisher.
    /// </summary>
    public string Publisher
    {
        get => Manifest?.Metadata.Identity.Publisher ?? string.Empty;
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.Identity.Publisher = value;
                OnPropertyChanged();
                ValidatePublisher();
            }
        }
    }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName
    {
        get => Manifest?.Metadata.DisplayName ?? string.Empty;
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.DisplayName = value;
                OnPropertyChanged();
                ValidateDisplayName();
            }
        }
    }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description
    {
        get => Manifest?.Metadata.Description ?? string.Empty;
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.Description = value;
                OnPropertyChanged();
                ValidateDescription();
            }
        }
    }

    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    public string Language
    {
        get => Manifest?.Metadata.Identity.Language ?? "en-US";
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.Identity.Language = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the more info URL.
    /// </summary>
    public string? MoreInfo
    {
        get => Manifest?.Metadata.MoreInfo;
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.MoreInfo = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the license.
    /// </summary>
    public string? License
    {
        get => Manifest?.Metadata.License;
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.License = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the icon path.
    /// </summary>
    public string? Icon
    {
        get => Manifest?.Metadata.Icon;
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.Icon = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the preview image path.
    /// </summary>
    public string? PreviewImage
    {
        get => Manifest?.Metadata.PreviewImage;
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.PreviewImage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public string? Tags
    {
        get => Manifest?.Metadata.Tags;
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.Tags = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the getting started guide URL.
    /// </summary>
    public string? GettingStartedGuide
    {
        get => Manifest?.Metadata.GettingStartedGuide;
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.GettingStartedGuide = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the release notes.
    /// </summary>
    public string? ReleaseNotes
    {
        get => Manifest?.Metadata.ReleaseNotes;
        set
        {
            if (Manifest != null)
            {
                Manifest.Metadata.ReleaseNotes = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Collections

    /// <summary>
    /// Gets the installation targets.
    /// </summary>
    public ObservableCollection<InstallationTarget> InstallationTargets =>
        Manifest?.InstallationTargets ?? new ObservableCollection<InstallationTarget>();

    /// <summary>
    /// Gets the dependencies.
    /// </summary>
    public ObservableCollection<Dependency> Dependencies =>
        Manifest?.Dependencies ?? new ObservableCollection<Dependency>();

    /// <summary>
    /// Gets the prerequisites.
    /// </summary>
    public ObservableCollection<Prerequisite> Prerequisites =>
        Manifest?.Prerequisites ?? new ObservableCollection<Prerequisite>();

    /// <summary>
    /// Gets the assets.
    /// </summary>
    public ObservableCollection<Asset> Assets =>
        Manifest?.Assets ?? new ObservableCollection<Asset>();

    /// <summary>
    /// Gets the contents (template declarations).
    /// </summary>
    public ObservableCollection<Content> Contents =>
        Manifest?.Contents ?? new ObservableCollection<Content>();

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates the ID field.
    /// </summary>
    private void ValidateId()
    {
        ClearErrors(nameof(Id));
        foreach (var error in ValidateRequired(Id, "ID"))
        {
            AddError(error, nameof(Id));
        }
    }

    /// <summary>
    /// Validates the Version field.
    /// </summary>
    private void ValidateVersion()
    {
        ClearErrors(nameof(Version));
        foreach (var error in ViewModelBase.ValidateVersion(Version, "Version"))
        {
            AddError(error, nameof(Version));
        }
    }

    /// <summary>
    /// Validates the Publisher field.
    /// </summary>
    private void ValidatePublisher()
    {
        ClearErrors(nameof(Publisher));
        foreach (var error in ValidateRequired(Publisher, "Publisher"))
        {
            AddError(error, nameof(Publisher));
        }
    }

    /// <summary>
    /// Validates the DisplayName field.
    /// </summary>
    private void ValidateDisplayName()
    {
        ClearErrors(nameof(DisplayName));
        foreach (var error in ValidateRequired(DisplayName, "Display Name"))
        {
            AddError(error, nameof(DisplayName));
        }
    }

    /// <summary>
    /// Validates the Description field.
    /// </summary>
    private void ValidateDescription()
    {
        ClearErrors(nameof(Description));
        foreach (var error in ValidateRequired(Description, "Description"))
        {
            AddError(error, nameof(Description));
        }
    }

    /// <summary>
    /// Validates all required fields.
    /// </summary>
    public void ValidateAll()
    {
        ValidateId();
        ValidateVersion();
        ValidatePublisher();
        ValidateDisplayName();
        ValidateDescription();
    }

    /// <summary>
    /// Gets all validation errors as a list of strings.
    /// </summary>
    /// <returns>A list of all error messages.</returns>
    public IReadOnlyList<string> GetAllErrors()
    {
        return GetErrors(null).Cast<string>().ToList();
    }

    #endregion

    /// <summary>
    /// Loads a manifest into the ViewModel.
    /// </summary>
    /// <param name="manifest">The manifest to load.</param>
    public void LoadManifest(VsixManifest manifest)
    {
        ClearAllErrors();
        Manifest = manifest;

        // Notify all properties changed
        OnPropertyChanged(nameof(Id));
        OnPropertyChanged(nameof(Version));
        OnPropertyChanged(nameof(Publisher));
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged(nameof(MoreInfo));
        OnPropertyChanged(nameof(License));
        OnPropertyChanged(nameof(Icon));
        OnPropertyChanged(nameof(PreviewImage));
        OnPropertyChanged(nameof(Tags));
        OnPropertyChanged(nameof(GettingStartedGuide));
        OnPropertyChanged(nameof(ReleaseNotes));
        OnPropertyChanged(nameof(InstallationTargets));
        OnPropertyChanged(nameof(Dependencies));
        OnPropertyChanged(nameof(Prerequisites));
        OnPropertyChanged(nameof(Assets));
        OnPropertyChanged(nameof(Contents));

        // Run initial validation
        ValidateAll();
    }
}
