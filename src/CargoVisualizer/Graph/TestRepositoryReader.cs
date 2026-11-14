namespace CargoVisualizer.Graph;

/// <summary>
/// Выполняет чтение и синтаксический разбор локального файла описания тестового графа зависимостей.
/// </summary>
public static class TestRepositoryReader
{
    private const int RequiredLineParts = 2;

    /// <summary>
    /// Считывает описание тестового репозитория из файла и возвращает словарь смежности пакетов.
    /// </summary>
    /// <param name="path">Путь к текстовому файлу с описанием графа зависимостей.</param>
    /// <returns>Словарь, сопоставляющий имени пакета список его прямых зависимостей.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если путь к файлу пустой.</exception>
    /// <exception cref="FileNotFoundException">Выбрасывается, если файл по указанному пути не найден.</exception>
    /// <exception cref="FormatException">
    /// Выбрасывается при нарушении формата строк, недопустимых именах пакетов или дублировании пакетов.
    /// </exception>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> Read(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Путь невалиден, ожидается непустая строка.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Файл тестового репозитория не найден: '{path}'.", path);
        }

        Dictionary<string, IReadOnlyList<string>> dict = new();
        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] parsedLine = ParseLine(line);
            string package = ValidatePackageName(parsedLine[0]);
            List<string> dependencies = ValidateDependencyList(ParseDependency(parsedLine[1]));

            if (dict.ContainsKey(package))
            {
                throw new FormatException($"Пакет '{package}' с зависимостями упомянут дважды.");
            }

            dict[package] = dependencies;
        }

        return dict;
    }

    private static string ValidatePackageName(string package)
    {
        string packageTrimmed = package.Trim();
        if (string.IsNullOrWhiteSpace(packageTrimmed))
        {
            throw new FormatException("Имя пакета пустое.");
        }

        foreach (char c in packageTrimmed)
        {
            if (!char.IsAsciiLetterUpper(c))
            {
                throw new FormatException("Имя пакета невалидно. Ожидаются заглавные латинские буквы.");
            }
        }

        return packageTrimmed;
    }

    private static List<string> ValidateDependencyList(string[] dependencyList)
    {
        List<string> list = new();
        foreach (string dependency in dependencyList)
        {
            string validatedDependency = ValidatePackageName(dependency);
            if (list.Contains(validatedDependency))
            {
                continue;
            }

            list.Add(validatedDependency);
        }

        return list;
    }

    private static string[] ParseDependency(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return Array.Empty<string>();
        }

        return line.Split(',');
    }

    private static string[] ParseLine(string line)
    {
        string[] parts = line.Split(':');
        if (parts.Length != RequiredLineParts)
        {
            throw new FormatException("Некорректная строка, ожидается один разделитель ':'.");
        }

        return parts;
    }
}