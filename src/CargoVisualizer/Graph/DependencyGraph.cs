namespace CargoVisualizer.Graph;

/// <summary>
/// Представляет ориентированный граф зависимостей пакетов на основе списка смежности.
/// </summary>
public class DependencyGraph
{
    private readonly Dictionary<string, List<string>> _packageDependenciesDictionary = new();

    /// <summary>
    /// Имя корневого (стартового) анализируемого пакета.
    /// </summary>
    public string RootPackage { get; }

    /// <summary>
    /// Коллекция имен всех пакетов (вершин), зарегистрированных в графе.
    /// </summary>
    public IReadOnlyCollection<string> Packages => _packageDependenciesDictionary.Keys;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="DependencyGraph"/>.
    /// </summary>
    /// <param name="rootPackageName">Имя корневого пакета графа.</param>
    /// <exception cref="ArgumentException">Выбрасывается, если имя корневого пакета пустое.</exception>
    public DependencyGraph(string rootPackageName)
    {
        RootPackage = ValidatePackageName(rootPackageName, nameof(rootPackageName));
    }

    /// <summary>
    /// Проверяет, зарегистрирован ли указанный пакет в графе.
    /// </summary>
    /// <param name="packageName">Имя проверяемого пакета.</param>
    /// <returns><c>true</c>, если узел присутствует в графе; иначе <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если имя пакета пустое.</exception>
    public bool ContainsPackage(string packageName)
    {
        packageName = ValidatePackageName(packageName, nameof(packageName));
        return _packageDependenciesDictionary.ContainsKey(packageName);
    }

    /// <summary>
    /// Возвращает список прямых зависимостей указанного пакета.
    /// </summary>
    /// <param name="packageName">Имя пакета, для которого запрашиваются зависимости.</param>
    /// <returns>Список имен прямых зависимостей только для чтения.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если имя пакета пустое.</exception>
    /// <exception cref="KeyNotFoundException">Выбрасывается, если пакет отсутствует в графе.</exception>
    public IReadOnlyList<string> GetDirectDependencies(string packageName)
    {
        packageName = ValidatePackageName(packageName, nameof(packageName));
        if (!ContainsPackage(packageName))
        {
            throw new KeyNotFoundException(
                $"Не удалось получить зависимости: пакет '{packageName}' отсутствует в графе.");
        }

        return _packageDependenciesDictionary[packageName];
    }

    /// <summary>
    /// Регистрирует пакет как вершину графа, если он еще не был добавлен.
    /// </summary>
    /// <param name="packageName">Имя добавляемого пакета.</param>
    /// <exception cref="ArgumentException">Выбрасывается, если имя пакета пустое.</exception>
    public void AddPackage(string packageName)
    {
        packageName = ValidatePackageName(packageName, nameof(packageName));
        _packageDependenciesDictionary.TryAdd(packageName, new List<string>());
    }

    /// <summary>
    /// Добавляет ориентированное ребро зависимости от <paramref name="packageName"/> к <paramref name="dependencyName"/>.
    /// </summary>
    /// <param name="packageName">Имя исходного пакета.</param>
    /// <param name="dependencyName">Имя пакета, от которого зависит исходный пакет.</param>
    /// <exception cref="ArgumentException">Выбрасывается, если одно из имен пакетов пустое.</exception>
    public void AddDependency(string packageName, string dependencyName)
    {
        packageName = ValidatePackageName(packageName, nameof(packageName));
        dependencyName = ValidatePackageName(dependencyName, nameof(dependencyName));
        if (!ContainsPackage(packageName))
        {
            AddPackage(packageName);
        }

        if (_packageDependenciesDictionary[packageName].Contains(dependencyName))
        {
            return;
        }

        _packageDependenciesDictionary[packageName].Add(dependencyName);
    }

    private static string ValidatePackageName(string packageName, string paramName)
    {
        if (string.IsNullOrWhiteSpace(packageName))
        {
            throw new ArgumentException("Передано пустое значение имени зависимости", paramName);
        }

        return packageName.Trim();
    }
}