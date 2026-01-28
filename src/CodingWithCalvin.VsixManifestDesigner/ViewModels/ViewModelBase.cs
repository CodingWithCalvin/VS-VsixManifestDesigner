using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CodingWithCalvin.VsixManifestDesigner.ViewModels;

/// <summary>
/// Base class for ViewModels implementing INotifyPropertyChanged and INotifyDataErrorInfo.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Gets a value indicating whether there are any validation errors.
    /// </summary>
    public bool HasErrors => _errors.Count > 0;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the ErrorsChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property whose errors changed.</param>
    protected virtual void OnErrorsChanged([CallerMemberName] string? propertyName = null)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    /// <summary>
    /// Sets a property value and raises PropertyChanged if the value changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">The backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>True if the value changed; otherwise, false.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Sets a property value, validates it, and raises PropertyChanged if the value changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">The backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="validate">The validation function that returns error messages.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>True if the value changed; otherwise, false.</returns>
    protected bool SetPropertyWithValidation<T>(ref T field, T value, Func<T, IEnumerable<string>> validate, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);

        ClearErrors(propertyName);
        var errors = validate(value);
        foreach (var error in errors)
        {
            AddError(error, propertyName);
        }

        return true;
    }

    /// <summary>
    /// Gets the validation errors for a specific property or for the entire entity.
    /// </summary>
    /// <param name="propertyName">The name of the property to get errors for, or null/empty for entity-level errors.</param>
    /// <returns>The validation errors for the property or entity.</returns>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.SelectMany(e => e.Value);
        }

        return _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Adds a validation error for a property.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="propertyName">The property name.</param>
    protected void AddError(string error, [CallerMemberName] string? propertyName = null)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        if (!_errors.ContainsKey(propertyName))
        {
            _errors[propertyName] = new List<string>();
        }

        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// Clears all validation errors for a property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected void ClearErrors([CallerMemberName] string? propertyName = null)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        if (_errors.ContainsKey(propertyName))
        {
            _errors.Remove(propertyName);
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// Clears all validation errors for all properties.
    /// </summary>
    protected void ClearAllErrors()
    {
        var propertyNames = _errors.Keys.ToList();
        _errors.Clear();

        foreach (var propertyName in propertyNames)
        {
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// Validates a required string field.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="fieldName">The display name of the field.</param>
    /// <returns>A collection of error messages, empty if valid.</returns>
    protected static IEnumerable<string> ValidateRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield return $"{fieldName} is required.";
        }
    }

    /// <summary>
    /// Validates that a string is a valid version number.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="fieldName">The display name of the field.</param>
    /// <returns>A collection of error messages, empty if valid.</returns>
    protected static IEnumerable<string> ValidateVersion(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield return $"{fieldName} is required.";
            yield break;
        }

        if (!System.Version.TryParse(value, out _))
        {
            yield return $"{fieldName} must be a valid version number (e.g., 1.0.0).";
        }
    }
}
