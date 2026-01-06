using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using CodingWithCalvin.VsixManifestDesigner.Models;

namespace CodingWithCalvin.VsixManifestDesigner.Services;

/// <summary>
/// Service for integrating manifest changes with the VSIX project file.
/// Uses direct XML manipulation to avoid conflicts with loaded projects.
/// </summary>
public sealed class ProjectIntegrationService : IProjectIntegrationService
{
    private static readonly XNamespace MSBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

    /// <summary>
    /// Default output groups for standard assets.
    /// </summary>
    private const string DefaultOutputGroups = "BuiltProjectOutputGroup;BuiltProjectOutputGroupDependencies;GetCopyToOutputDirectoryItems;SatelliteDllsProjectOutputGroup";

    /// <summary>
    /// Default local-only output groups (debug symbols).
    /// </summary>
    private const string DefaultLocalOnlyOutputGroups = "DebugSymbolsProjectOutputGroup";

    /// <summary>
    /// Output groups for template assets.
    /// </summary>
    private const string TemplateOutputGroups = "TemplateProjectOutputGroup";

    /// <inheritdoc/>
    public Task<bool> AddProjectReferenceAsync(string vsixProjectPath, string referencedProjectPath, string? assetType = null)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(vsixProjectPath) || string.IsNullOrEmpty(referencedProjectPath))
            {
                return false;
            }

            if (!File.Exists(vsixProjectPath))
            {
                return false;
            }

            var doc = XDocument.Load(vsixProjectPath);
            var root = doc.Root;
            if (root == null)
            {
                return false;
            }

            var ns = GetNamespace(root);
            var relativePath = GetRelativePath(vsixProjectPath, referencedProjectPath);

            // Check if reference already exists
            var existingRef = FindProjectReference(root, ns, relativePath, referencedProjectPath);
            if (existingRef != null)
            {
                // Update metadata if needed
                UpdateProjectReferenceMetadata(existingRef, ns, assetType);
                doc.Save(vsixProjectPath);
                return false; // Already existed
            }

            // Find or create ItemGroup for ProjectReferences
            var itemGroup = FindOrCreateProjectReferenceItemGroup(root, ns);

            // Create the ProjectReference element
            var projectRef = CreateProjectReferenceElement(ns, relativePath, assetType);
            itemGroup.Add(projectRef);

            doc.Save(vsixProjectPath);
            return true;
        });
    }

    /// <inheritdoc/>
    public Task<bool> RemoveProjectReferenceAsync(string vsixProjectPath, string referencedProjectPath)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(vsixProjectPath) || string.IsNullOrEmpty(referencedProjectPath))
            {
                return false;
            }

            if (!File.Exists(vsixProjectPath))
            {
                return false;
            }

            var doc = XDocument.Load(vsixProjectPath);
            var root = doc.Root;
            if (root == null)
            {
                return false;
            }

            var ns = GetNamespace(root);
            var relativePath = GetRelativePath(vsixProjectPath, referencedProjectPath);

            var existingRef = FindProjectReference(root, ns, relativePath, referencedProjectPath);
            if (existingRef == null)
            {
                return false;
            }

            var parent = existingRef.Parent;
            existingRef.Remove();

            // Remove empty ItemGroup
            if (parent != null && !parent.HasElements)
            {
                parent.Remove();
            }

            doc.Save(vsixProjectPath);
            return true;
        });
    }

    /// <inheritdoc/>
    public Task<bool> HasProjectReferenceAsync(string vsixProjectPath, string referencedProjectPath)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(vsixProjectPath) || string.IsNullOrEmpty(referencedProjectPath))
            {
                return false;
            }

            if (!File.Exists(vsixProjectPath))
            {
                return false;
            }

            var doc = XDocument.Load(vsixProjectPath);
            var root = doc.Root;
            if (root == null)
            {
                return false;
            }

            var ns = GetNamespace(root);
            var relativePath = GetRelativePath(vsixProjectPath, referencedProjectPath);

            return FindProjectReference(root, ns, relativePath, referencedProjectPath) != null;
        });
    }

    /// <inheritdoc/>
    public Task<string?> AddFileToProjectAsync(string vsixProjectPath, string sourceFilePath, bool includeInVsix = true)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(vsixProjectPath) || string.IsNullOrEmpty(sourceFilePath))
            {
                return null;
            }

            if (!File.Exists(vsixProjectPath) || !File.Exists(sourceFilePath))
            {
                return null;
            }

            var projectDir = Path.GetDirectoryName(vsixProjectPath);
            if (string.IsNullOrEmpty(projectDir))
            {
                return null;
            }

            // Determine target path - copy file to project directory if not already there
            var fileName = Path.GetFileName(sourceFilePath);
            var targetPath = Path.Combine(projectDir, fileName);
            var relativePath = fileName;

            if (!sourceFilePath.StartsWith(projectDir, StringComparison.OrdinalIgnoreCase))
            {
                // Copy file to project directory
                if (!File.Exists(targetPath))
                {
                    File.Copy(sourceFilePath, targetPath);
                }
            }
            else
            {
                // File is already in project directory, calculate relative path
                relativePath = GetRelativePath(vsixProjectPath, sourceFilePath);
            }

            var doc = XDocument.Load(vsixProjectPath);
            var root = doc.Root;
            if (root == null)
            {
                return null;
            }

            var ns = GetNamespace(root);

            // Check if file is already in project (for non-SDK projects)
            // SDK-style projects auto-include files, so we only need to add metadata
            var isSdkStyle = root.Attribute("Sdk") != null;

            if (includeInVsix)
            {
                // Find or create ItemGroup for Content
                var itemGroup = FindOrCreateContentItemGroup(root, ns);

                // Check if item already exists
                var existingItem = FindContentItem(root, ns, relativePath);
                if (existingItem != null)
                {
                    // Update IncludeInVSIX metadata
                    SetOrAddChildElement(existingItem, ns, "IncludeInVSIX", "true");
                }
                else
                {
                    // Create new Content item
                    var contentElement = new XElement(ns + "Content",
                        new XAttribute("Include", relativePath),
                        new XElement(ns + "IncludeInVSIX", "true"));
                    itemGroup.Add(contentElement);
                }

                doc.Save(vsixProjectPath);
            }

            return relativePath;
        });
    }

    /// <inheritdoc/>
    public Task<bool> RemoveFileFromProjectAsync(string vsixProjectPath, string projectRelativePath, bool deleteFromDisk = false)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(vsixProjectPath) || string.IsNullOrEmpty(projectRelativePath))
            {
                return false;
            }

            if (!File.Exists(vsixProjectPath))
            {
                return false;
            }

            var doc = XDocument.Load(vsixProjectPath);
            var root = doc.Root;
            if (root == null)
            {
                return false;
            }

            var ns = GetNamespace(root);

            var existingItem = FindContentItem(root, ns, projectRelativePath);
            if (existingItem != null)
            {
                var parent = existingItem.Parent;
                existingItem.Remove();

                // Remove empty ItemGroup
                if (parent != null && !parent.HasElements)
                {
                    parent.Remove();
                }

                doc.Save(vsixProjectPath);
            }

            if (deleteFromDisk)
            {
                var projectDir = Path.GetDirectoryName(vsixProjectPath);
                if (!string.IsNullOrEmpty(projectDir))
                {
                    var fullPath = Path.Combine(projectDir, projectRelativePath);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
            }

            return true;
        });
    }

    /// <inheritdoc/>
    public string? GetVsixProjectPath(string manifestFilePath)
    {
        if (string.IsNullOrEmpty(manifestFilePath))
        {
            return null;
        }

        var manifestDir = Path.GetDirectoryName(manifestFilePath);
        if (string.IsNullOrEmpty(manifestDir))
        {
            return null;
        }

        // Look for .csproj file in the same directory
        var csprojFiles = Directory.GetFiles(manifestDir, "*.csproj");
        if (csprojFiles.Length == 1)
        {
            return csprojFiles[0];
        }

        // If multiple, try to find one that references the manifest
        foreach (var csproj in csprojFiles)
        {
            try
            {
                var content = File.ReadAllText(csproj);
                var manifestFileName = Path.GetFileName(manifestFilePath);
                if (content.Contains(manifestFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return csproj;
                }
            }
            catch
            {
                // Ignore read errors
            }
        }

        return csprojFiles.FirstOrDefault();
    }

    private static XNamespace GetNamespace(XElement root)
    {
        // SDK-style projects don't have a namespace
        var ns = root.GetDefaultNamespace();
        return ns == XNamespace.None ? XNamespace.None : ns;
    }

    private static string GetRelativePath(string fromPath, string toPath)
    {
        var fromDir = Path.GetDirectoryName(fromPath);
        if (string.IsNullOrEmpty(fromDir))
        {
            return toPath;
        }

        var fromUri = new Uri(fromDir + Path.DirectorySeparatorChar);
        var toUri = new Uri(toPath);

        var relativeUri = fromUri.MakeRelativeUri(toUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        return relativePath.Replace('/', Path.DirectorySeparatorChar);
    }

    private static XElement? FindProjectReference(XElement root, XNamespace ns, string relativePath, string absolutePath)
    {
        var projectRefs = root.Descendants(ns + "ProjectReference");

        foreach (var pr in projectRefs)
        {
            var include = pr.Attribute("Include")?.Value;
            if (string.IsNullOrEmpty(include))
            {
                continue;
            }

            // Check both relative and normalized paths
            if (string.Equals(include, relativePath, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(NormalizePath(include), NormalizePath(relativePath), StringComparison.OrdinalIgnoreCase))
            {
                return pr;
            }

            // Also check against project name
            var projectName = Path.GetFileNameWithoutExtension(absolutePath);
            var refProjectName = Path.GetFileNameWithoutExtension(include);
            if (string.Equals(projectName, refProjectName, StringComparison.OrdinalIgnoreCase))
            {
                return pr;
            }
        }

        return null;
    }

    private static XElement? FindContentItem(XElement root, XNamespace ns, string relativePath)
    {
        var contentItems = root.Descendants(ns + "Content")
            .Concat(root.Descendants(ns + "None"));

        foreach (var item in contentItems)
        {
            var include = item.Attribute("Include")?.Value;
            if (string.IsNullOrEmpty(include))
            {
                continue;
            }

            if (string.Equals(include, relativePath, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(NormalizePath(include), NormalizePath(relativePath), StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }

        return null;
    }

    private static XElement FindOrCreateProjectReferenceItemGroup(XElement root, XNamespace ns)
    {
        // Find existing ItemGroup with ProjectReference
        var existingGroup = root.Elements(ns + "ItemGroup")
            .FirstOrDefault(ig => ig.Elements(ns + "ProjectReference").Any());

        if (existingGroup != null)
        {
            return existingGroup;
        }

        // Create new ItemGroup
        var newGroup = new XElement(ns + "ItemGroup");

        // Insert after last ItemGroup or at the end
        var lastItemGroup = root.Elements(ns + "ItemGroup").LastOrDefault();
        if (lastItemGroup != null)
        {
            lastItemGroup.AddAfterSelf(newGroup);
        }
        else
        {
            root.Add(newGroup);
        }

        return newGroup;
    }

    private static XElement FindOrCreateContentItemGroup(XElement root, XNamespace ns)
    {
        // Find existing ItemGroup with Content
        var existingGroup = root.Elements(ns + "ItemGroup")
            .FirstOrDefault(ig => ig.Elements(ns + "Content").Any());

        if (existingGroup != null)
        {
            return existingGroup;
        }

        // Create new ItemGroup
        var newGroup = new XElement(ns + "ItemGroup");

        // Insert after last ItemGroup or at the end
        var lastItemGroup = root.Elements(ns + "ItemGroup").LastOrDefault();
        if (lastItemGroup != null)
        {
            lastItemGroup.AddAfterSelf(newGroup);
        }
        else
        {
            root.Add(newGroup);
        }

        return newGroup;
    }

    private static XElement CreateProjectReferenceElement(XNamespace ns, string relativePath, string? assetType)
    {
        var projectRef = new XElement(ns + "ProjectReference",
            new XAttribute("Include", relativePath));

        // Determine output groups based on asset type
        var outputGroups = GetOutputGroupsForAssetType(assetType);
        if (!string.IsNullOrEmpty(outputGroups))
        {
            projectRef.Add(new XElement(ns + "IncludeOutputGroupsInVSIX", outputGroups));
        }

        projectRef.Add(new XElement(ns + "IncludeOutputGroupsInVSIXLocalOnly", DefaultLocalOnlyOutputGroups));

        return projectRef;
    }

    private static void UpdateProjectReferenceMetadata(XElement projectRef, XNamespace ns, string? assetType)
    {
        var outputGroups = GetOutputGroupsForAssetType(assetType);
        if (!string.IsNullOrEmpty(outputGroups))
        {
            SetOrAddChildElement(projectRef, ns, "IncludeOutputGroupsInVSIX", outputGroups);
        }

        SetOrAddChildElement(projectRef, ns, "IncludeOutputGroupsInVSIXLocalOnly", DefaultLocalOnlyOutputGroups);
    }

    private static string GetOutputGroupsForAssetType(string? assetType)
    {
        if (string.IsNullOrEmpty(assetType))
        {
            return DefaultOutputGroups;
        }

        // Template assets need TemplateProjectOutputGroup
        if (AssetTypes.IsTemplate(assetType))
        {
            return TemplateOutputGroups;
        }

        return DefaultOutputGroups;
    }

    private static void SetOrAddChildElement(XElement parent, XNamespace ns, string elementName, string value)
    {
        var existing = parent.Element(ns + elementName);
        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            parent.Add(new XElement(ns + elementName, value));
        }
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('/', '\\').TrimStart('.', '\\');
    }
}
