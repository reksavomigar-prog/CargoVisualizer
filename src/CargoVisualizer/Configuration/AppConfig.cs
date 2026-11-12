namespace CargoVisualizer.Configuration;

/// <summary>
/// Представляет неизменяемую конфигурацию приложения для анализа зависимостей пакетов Cargo.
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Имя анализируемого пакета Rust.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// URL-адрес удаленного репозитория или локальный путь к файлу тестового репозитория.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Признак использования тестового репозитория вместо обращения к реальному реестру.
    /// </summary>
    public bool IsTestMode { get; }

    /// <summary>
    /// Анализируемая версия пакета.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Имя генерируемого файла изображения графа зависимостей.
    /// </summary>
    public string OutputFileName { get; }

    /// <summary>
    /// Максимальная глубина обхода графа зависимостей.
    /// </summary>
    public int MaxDepth { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="AppConfig"/>.
    /// </summary>
    /// <param name="name">Имя анализируемого пакета Rust.</param>
    /// <param name="url">URL-адрес репозитория или путь к тестовому файлу.</param>
    /// <param name="isTestMode">Значение true, если включен тестовый режим работы; иначе false.</param>
    /// <param name="version">Версия пакета для анализа.</param>
    /// <param name="outputFileName">Имя результирующего файла с изображением графа.</param>
    /// <param name="maxDepth">Предельная глубина анализа транзитивных зависимостей.</param>
    public AppConfig(
        string name,
        string url,
        bool isTestMode,
        string version,
        string outputFileName,
        int maxDepth)
    {
        Name = name;
        Url = url;
        IsTestMode = isTestMode;
        Version = version;
        OutputFileName = outputFileName;
        MaxDepth = maxDepth;
    }
}