namespace StudyHub.Infrastructure.Configuration;

/// <summary>
/// Carrega variáveis de um arquivo ".env" (procurando a partir do diretório atual e subindo)
/// para variáveis de ambiente do processo. Variáveis já definidas no ambiente NÃO são sobrescritas.
/// </summary>
public static class DotEnvLoader
{
    public static void Load(string? startDirectory = null)
    {
        var dir = new DirectoryInfo(startDirectory ?? Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            var file = Path.Combine(dir.FullName, ".env");
            if (File.Exists(file))
            {
                LoadFile(file);
                return;
            }
            dir = dir.Parent;
        }
    }

    private static void LoadFile(string path)
    {
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;

            var idx = line.IndexOf('=');
            if (idx <= 0) continue;

            var key = line[..idx].Trim();
            var value = line[(idx + 1)..].Trim();
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
            {
                value = value[1..^1];
            }

            if (Environment.GetEnvironmentVariable(key) is null)
                Environment.SetEnvironmentVariable(key, value);
        }
    }
}
