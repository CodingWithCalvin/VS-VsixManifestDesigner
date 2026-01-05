using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CodingWithCalvin.VsixManifestDesigner.Models;

namespace CodingWithCalvin.VsixManifestDesigner.Services;

/// <summary>
/// Service for loading and saving VSIX manifest files.
/// </summary>
public sealed class ManifestService
{
    private static readonly XNamespace VsixNs = "http://schemas.microsoft.com/developer/vsx-schema/2011";
    private static readonly XNamespace DesignNs = "http://schemas.microsoft.com/developer/vsx-schema-design/2011";

    /// <summary>
    /// Loads a VSIX manifest from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the manifest file.</param>
    /// <returns>The parsed manifest.</returns>
    public VsixManifest Load(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Manifest file not found.", filePath);
        }

        var doc = XDocument.Load(filePath);
        var root = doc.Root ?? throw new InvalidOperationException("Invalid manifest: no root element.");

        var manifest = new VsixManifest
        {
            Version = root.Attribute("Version")?.Value ?? "2.0.0"
        };

        // Parse Metadata
        var metadata = root.Element(VsixNs + "Metadata");
        if (metadata != null)
        {
            manifest.Metadata = ParseMetadata(metadata);
        }

        // Parse Installation
        var installation = root.Element(VsixNs + "Installation");
        if (installation != null)
        {
            foreach (var target in installation.Elements(VsixNs + "InstallationTarget"))
            {
                manifest.InstallationTargets.Add(ParseInstallationTarget(target));
            }
        }

        // Parse Dependencies
        var dependencies = root.Element(VsixNs + "Dependencies");
        if (dependencies != null)
        {
            foreach (var dep in dependencies.Elements(VsixNs + "Dependency"))
            {
                manifest.Dependencies.Add(ParseDependency(dep));
            }
        }

        // Parse Prerequisites
        var prerequisites = root.Element(VsixNs + "Prerequisites");
        if (prerequisites != null)
        {
            foreach (var prereq in prerequisites.Elements(VsixNs + "Prerequisite"))
            {
                manifest.Prerequisites.Add(ParsePrerequisite(prereq));
            }
        }

        // Parse Assets
        var assets = root.Element(VsixNs + "Assets");
        if (assets != null)
        {
            foreach (var asset in assets.Elements(VsixNs + "Asset"))
            {
                manifest.Assets.Add(ParseAsset(asset));
            }
        }

        return manifest;
    }

    /// <summary>
    /// Saves a VSIX manifest to the specified file path.
    /// </summary>
    /// <param name="manifest">The manifest to save.</param>
    /// <param name="filePath">The path to save to.</param>
    public void Save(VsixManifest manifest, string filePath)
    {
        if (manifest == null)
        {
            throw new ArgumentNullException(nameof(manifest));
        }

        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            CreateRootElement(manifest)
        );

        doc.Save(filePath);
    }

    private ManifestMetadata ParseMetadata(XElement metadata)
    {
        var result = new ManifestMetadata();

        var identity = metadata.Element(VsixNs + "Identity");
        if (identity != null)
        {
            result.Identity = new ExtensionIdentity
            {
                Id = identity.Attribute("Id")?.Value ?? string.Empty,
                Version = identity.Attribute("Version")?.Value ?? "1.0.0",
                Language = identity.Attribute("Language")?.Value ?? "en-US",
                Publisher = identity.Attribute("Publisher")?.Value ?? string.Empty
            };
        }

        result.DisplayName = metadata.Element(VsixNs + "DisplayName")?.Value ?? string.Empty;
        result.Description = metadata.Element(VsixNs + "Description")?.Value ?? string.Empty;
        result.MoreInfo = metadata.Element(VsixNs + "MoreInfo")?.Value;
        result.License = metadata.Element(VsixNs + "License")?.Value;
        result.GettingStartedGuide = metadata.Element(VsixNs + "GettingStartedGuide")?.Value;
        result.ReleaseNotes = metadata.Element(VsixNs + "ReleaseNotes")?.Value;
        result.Icon = metadata.Element(VsixNs + "Icon")?.Value;
        result.PreviewImage = metadata.Element(VsixNs + "PreviewImage")?.Value;
        result.Tags = metadata.Element(VsixNs + "Tags")?.Value;

        return result;
    }

    private InstallationTarget ParseInstallationTarget(XElement element)
    {
        var target = new InstallationTarget
        {
            Id = element.Attribute("Id")?.Value ?? string.Empty,
            Version = element.Attribute("Version")?.Value ?? string.Empty
        };

        var arch = element.Element(VsixNs + "ProductArchitecture");
        if (arch != null)
        {
            target.ProductArchitecture = arch.Value;
        }

        return target;
    }

    private Dependency ParseDependency(XElement element)
    {
        return new Dependency
        {
            Id = element.Attribute("Id")?.Value ?? string.Empty,
            DisplayName = element.Attribute("DisplayName")?.Value ?? string.Empty,
            Version = element.Attribute("Version")?.Value ?? string.Empty,
            Source = element.Attribute(DesignNs + "Source")?.Value ?? "Manual",
            Location = element.Attribute("Location")?.Value
        };
    }

    private Prerequisite ParsePrerequisite(XElement element)
    {
        return new Prerequisite
        {
            Id = element.Attribute("Id")?.Value ?? string.Empty,
            DisplayName = element.Attribute("DisplayName")?.Value ?? string.Empty,
            Version = element.Attribute("Version")?.Value ?? string.Empty
        };
    }

    private Asset ParseAsset(XElement element)
    {
        return new Asset
        {
            Type = element.Attribute("Type")?.Value ?? string.Empty,
            Source = element.Attribute(DesignNs + "Source")?.Value ?? "File",
            Path = element.Attribute("Path")?.Value ?? string.Empty,
            ProjectName = element.Attribute(DesignNs + "ProjectName")?.Value,
            TargetPath = element.Attribute(DesignNs + "TargetPath")?.Value,
            VsixSubPath = element.Attribute(DesignNs + "VsixSubPath")?.Value,
            Addressable = bool.TryParse(element.Attribute("Addressable")?.Value, out var addr) && addr
        };
    }

    private XElement CreateRootElement(VsixManifest manifest)
    {
        var root = new XElement(VsixNs + "PackageManifest",
            new XAttribute("Version", manifest.Version),
            new XAttribute(XNamespace.Xmlns + "d", DesignNs)
        );

        // Metadata
        root.Add(CreateMetadataElement(manifest.Metadata));

        // Installation
        if (manifest.InstallationTargets.Any())
        {
            root.Add(CreateInstallationElement(manifest.InstallationTargets));
        }

        // Dependencies
        if (manifest.Dependencies.Any())
        {
            root.Add(CreateDependenciesElement(manifest.Dependencies));
        }

        // Prerequisites
        if (manifest.Prerequisites.Any())
        {
            root.Add(CreatePrerequisitesElement(manifest.Prerequisites));
        }

        // Assets
        if (manifest.Assets.Any())
        {
            root.Add(CreateAssetsElement(manifest.Assets));
        }

        return root;
    }

    private XElement CreateMetadataElement(ManifestMetadata metadata)
    {
        var element = new XElement(VsixNs + "Metadata");

        element.Add(new XElement(VsixNs + "Identity",
            new XAttribute("Id", metadata.Identity.Id),
            new XAttribute("Version", metadata.Identity.Version),
            new XAttribute("Language", metadata.Identity.Language),
            new XAttribute("Publisher", metadata.Identity.Publisher)
        ));

        element.Add(new XElement(VsixNs + "DisplayName", metadata.DisplayName));
        element.Add(new XElement(VsixNs + "Description",
            new XAttribute(XNamespace.Xml + "space", "preserve"),
            metadata.Description));

        if (!string.IsNullOrEmpty(metadata.MoreInfo))
        {
            element.Add(new XElement(VsixNs + "MoreInfo", metadata.MoreInfo));
        }

        if (!string.IsNullOrEmpty(metadata.License))
        {
            element.Add(new XElement(VsixNs + "License", metadata.License));
        }

        if (!string.IsNullOrEmpty(metadata.GettingStartedGuide))
        {
            element.Add(new XElement(VsixNs + "GettingStartedGuide", metadata.GettingStartedGuide));
        }

        if (!string.IsNullOrEmpty(metadata.ReleaseNotes))
        {
            element.Add(new XElement(VsixNs + "ReleaseNotes", metadata.ReleaseNotes));
        }

        if (!string.IsNullOrEmpty(metadata.Icon))
        {
            element.Add(new XElement(VsixNs + "Icon", metadata.Icon));
        }

        if (!string.IsNullOrEmpty(metadata.PreviewImage))
        {
            element.Add(new XElement(VsixNs + "PreviewImage", metadata.PreviewImage));
        }

        if (!string.IsNullOrEmpty(metadata.Tags))
        {
            element.Add(new XElement(VsixNs + "Tags", metadata.Tags));
        }

        return element;
    }

    private XElement CreateInstallationElement(ObservableCollection<InstallationTarget> targets)
    {
        var element = new XElement(VsixNs + "Installation");

        foreach (var target in targets)
        {
            var targetElement = new XElement(VsixNs + "InstallationTarget",
                new XAttribute("Id", target.Id),
                new XAttribute("Version", target.Version)
            );

            if (!string.IsNullOrEmpty(target.ProductArchitecture))
            {
                targetElement.Add(new XElement(VsixNs + "ProductArchitecture", target.ProductArchitecture));
            }

            element.Add(targetElement);
        }

        return element;
    }

    private XElement CreateDependenciesElement(ObservableCollection<Dependency> dependencies)
    {
        var element = new XElement(VsixNs + "Dependencies");

        foreach (var dep in dependencies)
        {
            var depElement = new XElement(VsixNs + "Dependency",
                new XAttribute("Id", dep.Id),
                new XAttribute("DisplayName", dep.DisplayName),
                new XAttribute(DesignNs + "Source", dep.Source),
                new XAttribute("Version", dep.Version)
            );

            if (!string.IsNullOrEmpty(dep.Location))
            {
                depElement.Add(new XAttribute("Location", dep.Location));
            }

            element.Add(depElement);
        }

        return element;
    }

    private XElement CreatePrerequisitesElement(ObservableCollection<Prerequisite> prerequisites)
    {
        var element = new XElement(VsixNs + "Prerequisites");

        foreach (var prereq in prerequisites)
        {
            element.Add(new XElement(VsixNs + "Prerequisite",
                new XAttribute("Id", prereq.Id),
                new XAttribute("Version", prereq.Version),
                new XAttribute("DisplayName", prereq.DisplayName)
            ));
        }

        return element;
    }

    private XElement CreateAssetsElement(ObservableCollection<Asset> assets)
    {
        var element = new XElement(VsixNs + "Assets");

        foreach (var asset in assets)
        {
            var assetElement = new XElement(VsixNs + "Asset",
                new XAttribute("Type", asset.Type),
                new XAttribute(DesignNs + "Source", asset.Source),
                new XAttribute("Path", asset.Path)
            );

            if (!string.IsNullOrEmpty(asset.ProjectName))
            {
                assetElement.Add(new XAttribute(DesignNs + "ProjectName", asset.ProjectName));
            }

            if (!string.IsNullOrEmpty(asset.TargetPath))
            {
                assetElement.Add(new XAttribute(DesignNs + "TargetPath", asset.TargetPath));
            }

            if (!string.IsNullOrEmpty(asset.VsixSubPath))
            {
                assetElement.Add(new XAttribute(DesignNs + "VsixSubPath", asset.VsixSubPath));
            }

            if (asset.Addressable)
            {
                assetElement.Add(new XAttribute("Addressable", "true"));
            }

            element.Add(assetElement);
        }

        return element;
    }
}
