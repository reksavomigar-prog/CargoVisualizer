namespace CargoVisualizer.Graph;

/// <summary>
/// Реализует построение транзитивного графа зависимостей методом поиска в ширину (BFS) с рекурсией.
/// </summary>
public class DependencyGraphBuilder
{
    private const int InitialDepth = 0;
    private readonly IDependencyProvider _dependencyProvider;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="DependencyGraphBuilder"/>.
    /// </summary>
    /// <param name="dependencyProvider">Поставщик прямых зависимостей для пакетов.</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если поставщик зависимостей равен null.</exception>
    public DependencyGraphBuilder(IDependencyProvider dependencyProvider)
    {
        if (dependencyProvider == null)
        {
            throw new ArgumentNullException(nameof(dependencyProvider));
        }

        _dependencyProvider = dependencyProvider;
    }

    /// <summary>
    /// Асинхронно строит транзитивный граф зависимостей от указанного корневого пакета до заданной глубины.
    /// </summary>
    /// <param name="rootPackage">Имя стартового (корневого) пакета.</param>
    /// <param name="maxDepth">Максимальная глубина обхода графа.</param>
    /// <returns>Построенный граф зависимостей <see cref="DependencyGraph"/>.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если имя корневого пакета пустое.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Выбрасывается, если максимальная глубина меньше нуля.
    /// </exception>
    public async Task<DependencyGraph> BuildAsync(string rootPackage, int maxDepth)
    {
        if (maxDepth < InitialDepth)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDepth));
        }

        DependencyGraph dependencyGraph = new(rootPackage);
        Queue<string> initialQueue = new();
        initialQueue.Enqueue(dependencyGraph.RootPackage);
        await TraverseLevelAsync(dependencyGraph, initialQueue, InitialDepth, maxDepth);
        return dependencyGraph;
    }

    private async Task TraverseLevelAsync(
        DependencyGraph graph,
        Queue<string> currentLevelQueue,
        int currentDepth,
        int maxDepth)
    {
        if (currentLevelQueue.Count == 0)
        {
            return;
        }

        foreach (string package in currentLevelQueue)
        {
            graph.AddPackage(package);
        }

        if (currentDepth >= maxDepth)
        {
            return;
        }

        Queue<string> nextLevelQueue = new();
        while (currentLevelQueue.Count > 0)
        {
            string package = currentLevelQueue.Dequeue();
            IReadOnlyList<string> dependencies = await _dependencyProvider.GetDependenciesAsync(package);
            foreach (string dep in dependencies)
            {
                graph.AddDependency(package, dep);
                if (!graph.ContainsPackage(dep) && !nextLevelQueue.Contains(dep))
                {
                    nextLevelQueue.Enqueue(dep);
                }
            }
        }

        await TraverseLevelAsync(graph, nextLevelQueue, currentDepth + 1, maxDepth);
    }
}