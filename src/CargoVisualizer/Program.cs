using CargoVisualizer.Configuration;

namespace CargoVisualizer;

/// <summary>
/// Точка входа CLI-приложения визуализации графа зависимостей пакетов Cargo.
/// </summary>
internal static class Program
{
    private const string DefaultConfigPath = "config.csv";
    private const int MaxArgsCount = 1;

    /// <summary>
    /// Запускает чтение конфигурации, валидацию параметров и вывод настроек в консоль.
    /// </summary>
    /// <param name="args">Аргументы командной строки (опциональный путь к CSV-файлу конфигурации).</param>
    /// <returns>Код завершения процесса: 0 при успехе, 1 при ошибке конфигурации.</returns>
    private static int Main(string[] args)
    {
        string path;
        try
        {
            if (args.Length == 0)
            {
                path = DefaultConfigPath;
            }
            else if (args.Length == MaxArgsCount)
            {
                path = args[0];
            }
            else
            {
                throw new ArgumentException(
                    $"Превышено количество аргументов. CargoVisualizer не принимает больше {MaxArgsCount} аргумента(-ов)");
            }

            var dict = CsvConfigReader.Read(path);
            var appConfig = ConfigValidator.Validate(dict);
            Console.WriteLine($"Name: {appConfig.Name}");
            Console.WriteLine($"Url: {appConfig.Url}");
            Console.WriteLine($"IsTestMode: {appConfig.IsTestMode}");
            Console.WriteLine($"Version: {appConfig.Version}");
            Console.WriteLine($"OutputFileName: {appConfig.OutputFileName}");
            Console.WriteLine($"MaxDepth: {appConfig.MaxDepth}");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return 1;
        }

        return 0;
    }
}