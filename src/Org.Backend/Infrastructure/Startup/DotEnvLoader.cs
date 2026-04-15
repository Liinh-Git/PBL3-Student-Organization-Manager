namespace Org.Backend.Infrastructure.Startup;

internal static class DotEnvLoader
{
    public static void LoadIfExists(string fileName = ".env")
    {
        var envPath = FindInCurrentOrParentDirectories(fileName);
        if (envPath is null)
        {
            return;
        }

        foreach (var rawLine in File.ReadLines(envPath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
            {
                line = line[7..].Trim();
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            var value = line[(separatorIndex + 1)..].Trim();
            value = Unquote(value);

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string? FindInCurrentOrParentDirectories(string fileName)
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string Unquote(string value)
    {
        if (value.Length < 2)
        {
            return value;
        }

        var startsAndEndsWithDoubleQuote = value[0] == '"' && value[^1] == '"';
        var startsAndEndsWithSingleQuote = value[0] == '\'' && value[^1] == '\'';

        return startsAndEndsWithDoubleQuote || startsAndEndsWithSingleQuote
            ? value[1..^1]
            : value;
    }
}
