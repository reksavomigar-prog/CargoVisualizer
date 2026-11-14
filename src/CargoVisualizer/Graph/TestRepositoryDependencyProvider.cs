namespace CargoVisualizer.Graph;

/// <summary>
/// Предоставляет адаптер поставщика зависимостей на основе локального словаря тестового репозитория.
/// </summary>
public class TestRepositoryDependencyProvider : IDependencyProvider
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _repository;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TestRepositoryDependencyProvider"/>.
    /// </summary>
    /// <param name="repository">Словарь смежности пакетов тестового репозитория.</param>
    /// <param name="rootPackage">Имя корневого пакета, с которого начинается обход графа.</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если словарь репозитория равен <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Выбрасывается, если имя корневого пакета пустое.</exception>
    /// <exception cref="KeyNotFoundException">
    /// Выбрасывается, если корневой пакет отсутствует в описании тестового репозитория.
    /// </exception>
    public TestRepositoryDependencyProvider(
        IReadOnlyDictionary<string, IReadOnlyList<string>> repository,
        string rootPackage)
    {
        rootPackage = ValidateString(rootPackage, nameof(rootPackage));
        if (repository == null)
        {
            throw new ArgumentNullException(nameof(repository));
        }

        if (!repository.ContainsKey(rootPackage))
        {
            throw new KeyNotFoundException("Корневого пакета нет в репозитории");
        }

        _repository = repository;
    }

    /// <summary>
    /// Возвращает список прямых зависимостей указанного пакета из тестового репозитория.
    /// </summary>
    /// <param name="packageName">Имя проверяемого пакета.</param>
    /// <returns>Задача, содержащая коллекцию имен прямых зависимостей только для чтения.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если имя пакета пустое.</exception>
    public Task<IReadOnlyList<string>> GetDependenciesAsync(string packageName)
    {
        packageName = ValidateString(packageName, nameof(packageName));
        if (_repository.TryGetValue(packageName, out IReadOnlyList<string>? dependencies))
        {
            return Task.FromResult(dependencies);
        }

        return Task.FromResult((IReadOnlyList<string>)Array.Empty<string>());
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