using CargoVisualizer.Dependencies;

namespace CargoVisualizer.Graph;

/// <summary>
/// Предоставляет адаптер поставщика зависимостей для взаимодействия с удаленным реестром пакетов Cargo.
/// </summary>
public class CargoRegistryDependencyProvider : IDependencyProvider
{
    private const string DevDependencyKind = "dev";
    private readonly CargoRegistryClient _client;
    private readonly string _baseUrl;
    private readonly string _rootPackage;
    private readonly string _rootVersion;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CargoRegistryDependencyProvider"/>.
    /// </summary>
    /// <param name="client">HTTP-клиент для выполнения запросов к реестру Cargo.</param>
    /// <param name="baseUrl">Базовый URL-адрес удаленного репозитория.</param>
    /// <param name="rootPackage">Имя корневого анализируемого пакета.</param>
    /// <param name="rootVersion">Конкретная версия корневого пакета.</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если HTTP-клиент равен <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Выбрасывается, если строковые параметры пусты.</exception>
    public CargoRegistryDependencyProvider(
        CargoRegistryClient client,
        string baseUrl,
        string rootPackage,
        string rootVersion)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        _baseUrl = ValidateString(baseUrl, nameof(baseUrl));
        _rootPackage = ValidateString(rootPackage, nameof(rootPackage));
        _rootVersion = ValidateString(rootVersion, nameof(rootVersion));
        _client = client;
    }

    /// <summary>
    /// Асинхронно получает список имен прямых рабочих зависимостей указанного пакета из реестра Cargo.
    /// </summary>
    /// <param name="packageName">Имя анализируемого пакета.</param>
    /// <returns>Коллекция имен прямых зависимостей (без учета dev-зависимостей).</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если имя пакета пустое.</exception>
    /// <exception cref="HttpRequestException">Выбрасывается при ошибке сетевого запроса к реестру.</exception>
    /// <exception cref="FormatException">Выбрасывается при нарушении формата JSON-ответа реестра.</exception>
    public async Task<IReadOnlyList<string>> GetDependenciesAsync(string packageName)
    {
        packageName = ValidateString(packageName, nameof(packageName));
        string requestResult;
        List<string> dependenciesList = new();
        if (packageName == _rootPackage)
        {
            requestResult = await _client.FetchDependenciesJsonAsync(_baseUrl, _rootPackage, _rootVersion);
        }
        else
        {
            string res = await _client.FetchCrateMetadataJsonAsync(_baseUrl, packageName);
            string version = CargoDependencyParser.ParseLatestVersion(res);
            requestResult = await _client.FetchDependenciesJsonAsync(_baseUrl, packageName, version);
        }

        foreach (var dependency in CargoDependencyParser.Parse(requestResult))
        {
            if (dependency.Kind != DevDependencyKind)
            {
                dependenciesList.Add(dependency.Name);
            }
        }

        return dependenciesList;
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