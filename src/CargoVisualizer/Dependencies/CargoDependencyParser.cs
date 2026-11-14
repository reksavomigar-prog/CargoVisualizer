using System.Text.Json;

namespace CargoVisualizer.Dependencies;

/// <summary>
/// Предоставляет методы для синтаксического анализа JSON-ответа реестра Cargo и извлечения метаданных пакетов.
/// </summary>
public static class CargoDependencyParser
{
    public const string KeyObject = "dependencies";
    public const string KeyName = "crate_id";
    public const string KeyVersionRequirement = "req";
    public const string KeyKind = "kind";
    public const string KeyCrate = "crate";
    public const string KeyMaxVersion = "max_version";
    public const string KeyMaxStableVersion = "max_stable_version";

    /// <summary>
    /// Разбирает JSON-строку ответа API реестра пакетов и формирует список прямых зависимостей.
    /// </summary>
    /// <param name="json">Сырая JSON-строка, полученная от удаленного реестра Cargo.</param>
    /// <returns>Коллекция прямых зависимостей <see cref="PackageDependency"/> только для чтения.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если входная строка JSON пуста.</exception>
    /// <exception cref="FormatException">
    /// Выбрасывается при поврежденном синтаксисе JSON или нарушении ожидаемой структуры данных.
    /// </exception>
    public static IReadOnlyList<PackageDependency> Parse(string json)
    {
        using JsonDocument parsedDoc = ValidateJson(json);

        JsonElement root = parsedDoc.RootElement;
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty(KeyObject, out JsonElement dependencyObjectsArray))
            {
                if (dependencyObjectsArray.ValueKind == JsonValueKind.Array)
                {
                    List<PackageDependency> dependencies = new();
                    foreach (JsonElement dependencyItem in dependencyObjectsArray.EnumerateArray())
                    {
                        PackageDependency packageDependency = ParseDependencyItem(dependencyItem);
                        dependencies.Add(packageDependency);
                    }

                    return dependencies;
                }

                throw new FormatException($"Массив {KeyObject} не валиден");
            }

            throw new FormatException($"Массив {KeyObject} не был найден");
        }

        throw new FormatException("Корневой объект не валиден");
    }

    /// <summary>
    /// Извлекает актуальную конкретную версию пакета из JSON-ответа метаданных крейта.
    /// </summary>
    /// <param name="json">Сырая JSON-строка ответа эндпоинта метаданных крейта.</param>
    /// <returns>Строка с наиболее актуальной версией пакета.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если входная строка JSON пуста.</exception>
    /// <exception cref="FormatException">
    /// Выбрасывается, если структура JSON нарушена или не удалось определить версию пакета.
    /// </exception>
    public static string ParseLatestVersion(string json)
    {
        using JsonDocument parsedDoc = ValidateJson(json);

        JsonElement root = parsedDoc.RootElement;
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty(KeyCrate, out JsonElement crateStruct))
            {
                if (crateStruct.ValueKind == JsonValueKind.Object)
                {
                    if (TryGetNonEmptyString(crateStruct, KeyMaxStableVersion, out string stableVersion))
                    {
                        return stableVersion;
                    }

                    if (TryGetNonEmptyString(crateStruct, KeyMaxVersion, out string latestVersion))
                    {
                        return latestVersion;
                    }

                    throw new FormatException($"Не удалось определить актуальную версию пакета в блоке '{KeyCrate}'.");
                }

                throw new FormatException($"Поле '{KeyCrate}' не является json-объектом");
            }

            throw new FormatException($"Поле '{KeyCrate}' не найдено в переданном файле");
        }

        throw new FormatException("Корневой объект не валиден");
    }

    private static JsonDocument ValidateJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Json строка не валидна, ожидается не пустая строка", nameof(json));
        }

        JsonDocument parsedDoc;
        try
        {
            parsedDoc = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            throw new FormatException("Поврежденный формат ответа реестра");
        }

        return parsedDoc;
    }

    private static string ValidateProperty(JsonElement obj, string propertyName)
    {
        if (obj.TryGetProperty(propertyName, out JsonElement property))
        {
            if (property.ValueKind == JsonValueKind.String)
            {
                return property.GetString() ?? "";
            }

            throw new FormatException($"Ключ {propertyName} не является строкой");
        }

        throw new FormatException($"Ключ {propertyName} не был найден в json");
    }

    private static bool TryGetNonEmptyString(JsonElement obj, string propertyName, out string value)
    {
        value = string.Empty;
        if (obj.TryGetProperty(propertyName, out JsonElement property)
            && property.ValueKind == JsonValueKind.String)
        {
            string? raw = property.GetString();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                value = raw.Trim();
                return true;
            }
        }

        return false;
    }

    private static PackageDependency ParseDependencyItem(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("Список зависимостей содержит не валидные данные. Ожидаются объекты");
        }

        PackageDependency packageDependency = new PackageDependency(
            ValidateProperty(item, KeyName),
            ValidateProperty(item, KeyVersionRequirement),
            ValidateProperty(item, KeyKind)
        );
        return packageDependency;
    }
}