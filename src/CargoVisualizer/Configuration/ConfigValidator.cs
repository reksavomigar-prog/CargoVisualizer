namespace CargoVisualizer.Configuration;

/// <summary>
/// Предоставляет методы для валидации параметров конфигурации
/// и формирования проверенного экземпляра <see cref="AppConfig"/>.
/// </summary>
public static class ConfigValidator
{
    private const string KeyName = "Name";
    private const string KeyUrl = "Url";
    private const string KeyIsTestMode = "IsTestMode";
    private const string KeyVersion = "Version";
    private const string KeyOutputFileName = "OutputFileName";
    private const string KeyMaxDepth = "MaxDepth";

    private static readonly string[] RequiredKeysArr =
    {
        KeyName, KeyUrl, KeyIsTestMode, KeyVersion, KeyOutputFileName, KeyMaxDepth
    };

    /// <summary>
    /// Проверяет словарь сырых параметров конфигурации и создает объект <see cref="AppConfig"/>.
    /// </summary>
    /// <param name="rawConfig">Словарь строковых параметров, считанных из файла конфигурации.</param>
    /// <returns>Валидированный и неизменяемый экземпляр <see cref="AppConfig"/>.</returns>
    /// <exception cref="KeyNotFoundException">Выбрасывается при отсутствии одного из обязательных ключей.</exception>
    /// <exception cref="FormatException">Выбрасывается при некорректном формате или значении параметра.</exception>
    public static AppConfig Validate(Dictionary<string, string> rawConfig)
    {
        EnsureRequiredKeys(rawConfig);
        string name = rawConfig[KeyName];
        string version = rawConfig[KeyVersion];
        bool isTestMode = ValidateIsTestMode(rawConfig[KeyIsTestMode]);
        string url = ValidateUrl(rawConfig[KeyUrl], isTestMode);
        string outputFileName = ValidateOutputFileName(rawConfig[KeyOutputFileName]);
        int maxDepth = ValidateMaxDepth(rawConfig[KeyMaxDepth]);
        return new AppConfig(name, url, isTestMode, version, outputFileName, maxDepth);
    }

    /// <summary>
    /// Проверяет наличие всех обязательных ключей и непустоту их значений.
    /// </summary>
    /// <param name="rawConfig">Словарь исходных параметров конфигурации.</param>
    /// <exception cref="KeyNotFoundException">Выбрасывается, если обязательный ключ отсутствует.</exception>
    /// <exception cref="FormatException">Выбрасывается, если значение параметра пустое.</exception>
    private static void EnsureRequiredKeys(Dictionary<string, string> rawConfig)
    {
        foreach (var key in RequiredKeysArr)
        {
            if (!rawConfig.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Ключ {key} не был указан");
            }

            if (string.IsNullOrWhiteSpace(rawConfig[key].Trim()))
            {
                throw new FormatException($"Словарь содержит невалидный ключ {key}, отсутствует значение");
            }
        }
    }

    /// <summary>
    /// Преобразует строковое значение флага тестового режима в логический тип.
    /// </summary>
    /// <param name="isTestMode">Строковое представление логического значения.</param>
    /// <returns>Значение <c>true</c> или <c>false</c>.</returns>
    /// <exception cref="FormatException">Выбрасывается, если строка не является корректным булевым значением.</exception>
    private static bool ValidateIsTestMode(string isTestMode)
    {
        if (bool.TryParse(isTestMode.Trim(), out bool result))
        {
            return result;
        }

        throw new FormatException("Словарь содержит невалидное значение режима, допустим только true/false");
    }

    /// <summary>
    /// Проверяет корректность URL-адреса репозитория или пути к тестовому файлу.
    /// </summary>
    /// <param name="url">URL-адрес репозитория или локальный путь.</param>
    /// <param name="isTestMode">Признак работы в тестовом режиме.</param>
    /// <returns>Проверенная строка адреса или пути.</returns>
    /// <exception cref="FormatException">Выбрасывается при некорректном формате HTTP/HTTPS-адреса.</exception>
    private static string ValidateUrl(string url, bool isTestMode)
    {
        if (!isTestMode)
        {
            bool isUri = Uri.TryCreate(url, UriKind.Absolute, out Uri? uri);
            if (isUri && uri is not null)
            {
                bool isSchemeCorrect = uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
                if (isSchemeCorrect)
                {
                    return url;
                }
            }

            throw new FormatException("Словарь содержит невалидное значение пути, невозможно выполнить запрос на сервер");
        }

        return url;
    }

    /// <summary>
    /// Проверяет отсутствие недопустимых символов файловой системы в имени выходного файла.
    /// </summary>
    /// <param name="outputFileName">Имя генерируемого файла графа.</param>
    /// <returns>Проверенное имя файла.</returns>
    /// <exception cref="FormatException">Выбрасывается при наличии запрещенных символов в имени файла.</exception>
    private static string ValidateOutputFileName(string outputFileName)
    {
        if (outputFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new FormatException("Словарь содержит невалидное название выходного файла");
        }

        return outputFileName;
    }

    /// <summary>
    /// Преобразует строковое значение максимальной глубины анализа в неотрицательное целое число.
    /// </summary>
    /// <param name="maxDepth">Строковое представление максимальной глубины.</param>
    /// <returns>Неотрицательное целое число.</returns>
    /// <exception cref="FormatException">Выбрасывается, если значение не является целым неотрицательным числом.</exception>
    private static int ValidateMaxDepth(string maxDepth)
    {
        if (int.TryParse(maxDepth.Trim(), out int result))
        {
            if (result >= 0)
            {
                return result;
            }

            throw new FormatException(
                "Словарь содержит невалидное значение максимальной глубины дерева, ожидается положительное число или 0");
        }

        throw new FormatException("Словарь содержит невалидное значение максимальной глубины дерева, ожидается число");
    }
}