namespace Microsoft.Extensions.Configuration;

/// <summary>
/// Extension methods for required configuration value access.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets a required configuration value and throws if it is missing.
    /// </summary>
    /// <param name="configuration">The configuration source.</param>
    /// <param name="name">The key name to resolve.</param>
    /// <returns>The configuration value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the value is missing.</exception>
    public static string GetRequiredValue(this IConfiguration configuration, string name) =>
        configuration[name] ?? throw new InvalidOperationException($"Configuration missing value for: {(configuration is IConfigurationSection s ? s.Path + ":" + name : name)}");
}
