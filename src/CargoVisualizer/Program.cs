using CargoVisualizer.Configuration;
using CargoVisualizer.Dependencies;
using CargoVisualizer.Graph;

namespace CargoVisualizer;

/// <summary>
/// Точка входа CLI-приложения визуализации графа зависимостей пакетов Cargo.
/// </summary>
internal static class Program
{
    private const string DefaultConfigPath = "config.csv";
    private const int MaxArgsCount = 1;

    /// <summary>
    /// Запускает загрузку конфигурации, построение транзитивного графа зависимостей и его вывод в консоль.
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

            IDependencyProvider dependencyProvider = ResolveDependencyProvider(appConfig);
            DependencyGraphBuilder graphBuilder = new(dependencyProvider);
            DependencyGraph dependencyGraph = await graphBuilder.BuildAsync(appConfig.Name, appConfig.MaxDepth);
            WriteDependencyGraph(dependencyGraph);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Выводит в стандартный поток вывода построенный граф зависимостей в виде списка смежности.
    /// </summary>
    /// <param name="dependencyGraph">Построенный граф зависимостей пакетов.</param>
    private static void WriteDependencyGraph(DependencyGraph dependencyGraph)
    {
        Console.WriteLine("==============DEPENDENCIES TREE==============");
        foreach (var package in dependencyGraph.Packages)
        {
            string dependencies = string.Join(", ", dependencyGraph.GetDirectDependencies(package));

            if (string.IsNullOrWhiteSpace(dependencies))
            {
                Console.WriteLine($"{package} -> (нет зависимостей)");
                continue;
            }

            Console.WriteLine($"{package} -> {dependencies}");
        }
    }

    /// <summary>
    /// Создает поставщика прямых зависимостей в соответствии с выбранным режимом работы приложения.
    /// </summary>
    /// <param name="config">Проверенная конфигурация приложения.</param>
    /// <returns>Реализация <see cref="IDependencyProvider"/> для тестового или удаленного репозитория.</returns>
    private static IDependencyProvider ResolveDependencyProvider(AppConfig config)
    {
        if (config.IsTestMode)
        {
            var dict = TestRepositoryReader.Read(config.Url);
            return new TestRepositoryDependencyProvider(dict, config.Name);
        }
        else
        {
            CargoRegistryClient client = new();
            return new CargoRegistryDependencyProvider(client, config.Url, config.Name, config.Version);
        }
    }
}