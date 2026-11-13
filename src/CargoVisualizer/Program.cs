using CargoVisualizer.Configuration;
using CargoVisualizer.Dependencies;

namespace CargoVisualizer;

/// <summary>
/// Точка входа CLI-приложения визуализации графа зависимостей пакетов Cargo.
/// </summary>
internal static class Program
{
    private const string DefaultConfigPath = "config.csv";
    private const int MaxArgsCount = 1;

    /// <summary>
    /// Запускает загрузку конфигурации, получение метаданных из реестра Cargo и вывод прямых зависимостей.
    /// </summary>
    /// <param name="args">Аргументы командной строки (опциональный путь к CSV-файлу конфигурации).</param>
    /// <returns>Код завершения процесса: 0 при успехе, 1 при ошибке конфигурации, сети или формата данных.</returns>
    private static async Task<int> Main(string[] args)
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
                    "Превышено количество аргументов. " +
                    $"CargoVisualizer принимает не более {MaxArgsCount} аргумента(-ов)");
            }

            var dict = CsvConfigReader.Read(path);
            var appConfig = ConfigValidator.Validate(dict);
            CargoRegistryClient cargoRegistryClient = new();
            string fetchedDependenciesJson = await cargoRegistryClient.FetchDependenciesJsonAsync(
                appConfig.Url,
                appConfig.Name,
                appConfig.Version);
            IReadOnlyList<PackageDependency> dependenciesList = CargoDependencyParser.Parse(fetchedDependenciesJson);
            WriteDependencyList(appConfig, dependenciesList);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Выводит в стандартный поток вывода таблицу прямых зависимостей анализируемого пакета.
    /// </summary>
    /// <param name="appConfig">Проверенная конфигурация приложения с именем и версией пакета.</param>
    /// <param name="dependencyList">Список прямых зависимостей пакета.</param>
    private static void WriteDependencyList(
        AppConfig appConfig,
        IReadOnlyList<PackageDependency> dependencyList)
    {
        Console.WriteLine($"Прямые зависимости пакета {appConfig.Name} ({appConfig.Version}):");
        if (!dependencyList.Any())
        {
            Console.WriteLine("Прямые зависимости отсутствуют.");
            return;
        }

        Console.WriteLine("Имя | Версия | Тип зависимости");
        foreach (var dep in dependencyList)
        {
            Console.WriteLine($"{dep.Name} | {dep.VersionRequirement} | {dep.Kind}");
        }
    }
}