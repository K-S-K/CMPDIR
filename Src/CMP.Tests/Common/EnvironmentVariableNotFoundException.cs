namespace CMP.Tests.Common;

/// <summary>
/// Environment Variable Not Found Exception
/// </summary>
/// <remarks>
/// This exception is thrown when an environment variable is not found.
/// </remarks>
public class EnvironmentVariableNotFoundException(string variableName)
: Exception($"Environment variable {variableName} not found.")
{
    /// <summary>
    /// Throws EnvironmentVariableNotFoundException
    /// </summary>
    public static void Throw(string variableName)
       => throw new EnvironmentVariableNotFoundException(variableName);

    /// <summary>
    /// Throws EnvironmentVariableNotFoundException if variableValue is null
    /// </summary>
    /// <param name="variableValue">Variable Value</param>
    public static void ThrowIfNull(string? variableValue, string variableName)
    {
        if (string.IsNullOrWhiteSpace(variableValue))
        {
            Throw(variableName);
        }
    }
}
