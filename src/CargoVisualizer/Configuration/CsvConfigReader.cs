namespace CargoVisualizer.Configuration;

/// <summary>
/// Предоставляет функциональность для чтения и синтаксического анализа конфигурационных файлов формата CSV.
/// </summary>
public static class CsvConfigReader
{
    /// <summary>
    /// Считывает файл конфигурации и формирует словарь параметров в формате «ключ — значение».
    /// </summary>
    /// <param name="filepath">Относительный или абсолютный путь к файлу конфигурации CSV.</param>
    /// <returns>Словарь пар «ключ — значение» без учета регистра символов в именах параметров.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если путь к файлу пуст или содержит только пробелы.</exception>
    /// <exception cref="FileNotFoundException">Выбрасывается, если файл по указанному пути не найден на диске.</exception>
    /// <exception cref="FormatException">Выбрасывается при отсутствии разделителя, пустом ключе/значении или дублировании ключей.</exception>
    public static Dictionary<string, string> Read(string filepath)
    {
        if (string.IsNullOrWhiteSpace(filepath))
        {
            throw new ArgumentException("Путь к файлу не был задан");
        }

        if (!File.Exists(filepath))
        {
            throw new FileNotFoundException("Файл по заданному пути не найден", filepath);
        }

        Dictionary<string, string> parsedDataDictionary = new(StringComparer.OrdinalIgnoreCase);
        string[] lines = File.ReadAllLines(filepath);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] lineParts = line.Split(',', 2);
            if (lineParts.Length != 2)
            {
                throw new FormatException($"Файл содержит невалидную строку, отсутствует разделитель: '{line}'");
            }

            string key = lineParts[0].Trim();
            string value = lineParts[1].Trim();

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new FormatException("Файл содержит невалидную строку, отсутствует ключ");
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException("Файл содержит невалидную строку, отсутствует значение");
            }

            if (parsedDataDictionary.ContainsKey(key))
            {
                throw new FormatException($"Файл содержит дублирующийся ключ: '{key}'");
            }

            parsedDataDictionary[key] = value;
        }

        return parsedDataDictionary;
    }
}