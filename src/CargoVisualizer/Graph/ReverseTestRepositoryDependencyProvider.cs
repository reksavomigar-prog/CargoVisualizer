namespace CargoVisualizer.Graph;

/// <summary>
/// Предоставляет адаптер поставщика обратных зависимостей на основе локального словаря тестового репозитория.
/// </summary>
public class ReverseTestRepositoryDependencyProvider : IDependencyProvider
{
    private readonly Dictionary<string, List<string>> _repository;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ReverseTestRepositoryDependencyProvider"/>.
    /// </summary>
    /// <param name="repository">Словарь прямых зависимостей пакетов тестового репозитория.</param>
    /// <param name="targetPackage">Имя целевого пакета, для которого ищутся обратные зависимости.</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если словарь репозитория равен <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Выбрасывается, если имя целевого пакета пустое.</exception>
    /// <exception cref="KeyNotFoundException">
    /// Выбрасывается, если целевой пакет отсутствует в описании тестового репозитория.
    /// </exception>
    public ReverseTestRepositoryDependencyProvider(
        IReadOnlyDictionary<string, IReadOnlyList<string>> repository,
        string targetPackage)
    {
        targetPackage = ValidateString(targetPackage, nameof(targetPackage));
        if (repository == null)
        {
            throw new ArgumentNullException(nameof(repository));
        }

        _repository = BuildReverseRepository(repository);

        if (!_repository.ContainsKey(targetPackage))
        {
            throw new KeyNotFoundException("Корневого пакета нет в репозитории");
        }
    }

    /// <summary>
    /// Возвращает список пакетов, которые напрямую зависят от указанного пакета.
    /// </summary>
    /// <param name="packageName">Имя проверяемого пакета.</param>
    /// <returns>Задача, содержащая коллекцию имен пакетов, зависящих от указанного пакета.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если имя пакета пустое.</exception>
    public Task<IReadOnlyList<string>> GetDependenciesAsync(string packageName)
    {
        packageName = ValidateString(packageName, nameof(packageName));
        if (_repository.TryGetValue(packageName, out List<string>? dependencies))
        {
            return Task.FromResult((IReadOnlyList<string>)dependencies);
        }

        return Task.FromResult((IReadOnlyList<string>)Array.Empty<string>());
    }

    private static Dictionary<string, List<string>> BuildReverseRepository(
        IReadOnlyDictionary<string, IReadOnlyList<string>> forwardRepository)
    {
        Dictionary<string, List<string>> reversedDictionary = new();
        foreach (var package in forwardRepository.Keys)
        {
            reversedDictionary.TryAdd(package, new List<string>());
        }

        foreach (var packageDependencies in forwardRepository)
        {
            foreach (string dependency in packageDependencies.Value)
            {
                AppendConnection(reversedDictionary, packageDependencies.Key, dependency);
            }
        }

        return reversedDictionary;
    }

    private static void AppendConnection(Dictionary<string, List<string>> repository, string value, string key)
    {
        if (repository.ContainsKey(key))
        {
            if (!repository[key].Contains(value))
            {
                repository[key].Add(value);
            }
        }
        else
        {
            repository[key] = new List<string>() { value };
        }
    }

    private static string ValidateString(string param, string paramName)
    {
        if (string.IsNullOrWhiteSpace(param))
        {
            throw new ArgumentException("В конструктор передана пустая строка", paramName);
        }

        return param.Trim();
    }
}