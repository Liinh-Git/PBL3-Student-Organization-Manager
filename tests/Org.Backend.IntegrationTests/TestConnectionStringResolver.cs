namespace Org.Backend.IntegrationTests;

internal static class TestConnectionStringResolver
{
    private const string EnvVarName = "TEST_DB_CONNECTION";

    public static string Resolve()
    {
        var value = Environment.GetEnvironmentVariable(EnvVarName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing required environment variable '{EnvVarName}'. " +
                "Set it to a valid PostgreSQL connection string before running integration tests.");
        }

        var trimmed = value.Trim();
        if (trimmed.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Environment variable '{EnvVarName}' contains a placeholder value. " +
                "Provide a real local PostgreSQL connection string.");
        }

        if (!trimmed.Contains("Host=", StringComparison.OrdinalIgnoreCase)
            || !trimmed.Contains("Database=", StringComparison.OrdinalIgnoreCase)
            || !trimmed.Contains("Username=", StringComparison.OrdinalIgnoreCase)
            || !trimmed.Contains("Password=", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Environment variable '{EnvVarName}' is not a valid PostgreSQL connection string. " +
                "Expected keys: Host, Database, Username, Password.");
        }

        return trimmed;
    }
}
