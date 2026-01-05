using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodingWithCalvin.VsixManifestDesigner.Services;

/// <summary>
/// Service for querying MSBuild properties and targets from project files.
/// Uses direct XML parsing for simplicity and thread-safety.
/// </summary>
public sealed class MsBuildQueryService : IMsBuildQueryService
{
    private static readonly XNamespace MsBuildNs = "http://schemas.microsoft.com/developer/msbuild/2003";

    /// <inheritdoc />
    public async Task<string?> GetPropertyValueAsync(string projectPath, string propertyName)
    {
        if (string.IsNullOrEmpty(projectPath))
        {
            throw new ArgumentException("Project path cannot be null or empty.", nameof(projectPath));
        }

        if (string.IsNullOrEmpty(propertyName))
        {
            throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));
        }

        if (!File.Exists(projectPath))
        {
            return null;
        }

        return await Task.Run(() =>
        {
            try
            {
                var doc = XDocument.Load(projectPath);
                var root = doc.Root;

                if (root == null)
                {
                    return null;
                }

                // Check if SDK-style (no namespace) or legacy (with namespace)
                var isSdkStyle = root.Attribute("Sdk") != null ||
                                 string.IsNullOrEmpty(root.Name.NamespaceName);

                // Search for the property in PropertyGroup elements
                var propertyGroups = isSdkStyle
                    ? root.Elements("PropertyGroup")
                    : root.Elements(MsBuildNs + "PropertyGroup");

                foreach (var propertyGroup in propertyGroups)
                {
                    var property = isSdkStyle
                        ? propertyGroup.Element(propertyName)
                        : propertyGroup.Element(MsBuildNs + propertyName);

                    if (property != null)
                    {
                        return property.Value;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        });
    }

    /// <inheritdoc />
    public async Task<bool> HasTargetAsync(string projectPath, string targetName)
    {
        if (string.IsNullOrEmpty(projectPath))
        {
            throw new ArgumentException("Project path cannot be null or empty.", nameof(projectPath));
        }

        if (string.IsNullOrEmpty(targetName))
        {
            throw new ArgumentException("Target name cannot be null or empty.", nameof(targetName));
        }

        if (!File.Exists(projectPath))
        {
            return false;
        }

        return await Task.Run(() =>
        {
            try
            {
                var doc = XDocument.Load(projectPath);
                var root = doc.Root;

                if (root == null)
                {
                    return false;
                }

                // Check if SDK-style (no namespace) or legacy (with namespace)
                var isSdkStyle = root.Attribute("Sdk") != null ||
                                 string.IsNullOrEmpty(root.Name.NamespaceName);

                // Search for the target
                var targets = isSdkStyle
                    ? root.Elements("Target")
                    : root.Elements(MsBuildNs + "Target");

                var hasTarget = targets.Any(t =>
                    string.Equals(t.Attribute("Name")?.Value, targetName, StringComparison.OrdinalIgnoreCase));

                if (hasTarget)
                {
                    return true;
                }

                // Also check imported targets by looking for known SDK imports
                // For TemplateProjectOutputGroup, it's typically provided by:
                // - Microsoft.VSSDK.BuildTools
                // - CodingWithCalvin.VsixSdk
                if (targetName.Equals("TemplateProjectOutputGroup", StringComparison.OrdinalIgnoreCase))
                {
                    var content = File.ReadAllText(projectPath);
                    return content.Contains("Microsoft.VSSDK.BuildTools", StringComparison.OrdinalIgnoreCase) ||
                           content.Contains("Microsoft.VsSDK", StringComparison.OrdinalIgnoreCase) ||
                           content.Contains("CodingWithCalvin.VsixSdk", StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }
            catch
            {
                return false;
            }
        });
    }
}
