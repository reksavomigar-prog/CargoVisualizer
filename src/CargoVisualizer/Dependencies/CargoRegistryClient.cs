using System.Net;

namespace CargoVisualizer.Dependencies;

/// <summary>
/// Предоставляет HTTP-клиент для получения метаданных о зависимостях и версиях пакетов из реестра Cargo.
/// </summary>
public class CargoRegistryClient
{
    private static readonly HttpClient Client;

    static CargoRegistryClient()
    {
        Client = new HttpClient();
        Client.DefaultRequestHeaders.UserAgent.ParseAdd("CargoVisualizer/1.0");
    }

    /// <summary>
    /// Асинхронно запрашивает у удаленного реестра Cargo JSON-документ с прямыми зависимостями пакета.
    /// </summary>
    /// <param name="baseUrl">Базовый URL-адрес репозитория (например, <c>https://crates.io</c>).</param>
    /// <param name="packageName">Имя анализируемого пакета.</param>
    /// <param name="version">Конкретная версия пакета.</param>
    /// <returns>Сырая JSON-строка ответа от API реестра пакетов.</returns>
    /// <exception cref="ArgumentException">
    /// Выбрасывается, если URL репозитория, имя пакета или версия некорректны или пусты.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Выбрасывается при сбое сетевого соединения, отсутствии пакета в реестре или коде ошибки HTTP.
    /// </exception>
    public async Task<string> FetchDependenciesJsonAsync(
        string baseUrl,
        string packageName,
        string version)
    {
        string validatedUrl = ValidateUrl(baseUrl);
        string escapedName = ValidateName(packageName);
        string escapedVersion = ValidateVersion(version);

        string requestUrl = $"{validatedUrl}/api/v1/crates/{escapedName}/{escapedVersion}/dependencies";
        return await SendGetRequestAsync(requestUrl);
    }

    /// <summary>
    /// Асинхронно запрашивает у удаленного реестра Cargo JSON-документ с общими метаданными пакета.
    /// </summary>
    /// <param name="baseUrl">Базовый URL-адрес репозитория (например, <c>https://crates.io</c>).</param>
    /// <param name="packageName">Имя анализируемого пакета.</param>
    /// <returns>Сырая JSON-строка метаданных пакета.</returns>
    /// <exception cref="ArgumentException">
    /// Выбрасывается, если URL репозитория или имя пакета некорректны или пусты.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Выбрасывается при сбое сетевого соединения, отсутствии пакета в реестре или коде ошибки HTTP.
    /// </exception>
    public async Task<string> FetchCrateMetadataJsonAsync(string baseUrl, string packageName)
    {
        string validatedUrl = ValidateUrl(baseUrl);
        string escapedName = ValidateName(packageName);

        string requestUrl = $"{validatedUrl}/api/v1/crates/{escapedName}";
        return await SendGetRequestAsync(requestUrl);
    }

    private static async Task<string> SendGetRequestAsync(string requestUrl)
    {
        HttpResponseMessage response;
        try
        {
            response = await Client.GetAsync(requestUrl);
        }
        catch
        {
            throw new HttpRequestException("Не удалось установить соединение с сервером");
        }

        ValidateResponse(response);
        return await response.Content.ReadAsStringAsync();
    }

    private static string ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException(
                "Значение адреса не валидно, ожидается не пустая строка",
                nameof(url));
        }

        bool isUri = Uri.TryCreate(url, UriKind.Absolute, out Uri? uri);
        if (isUri && uri is not null)
        {
            bool isSchemeCorrect = uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            if (isSchemeCorrect)
            {
                return url.TrimEnd('/');
            }
        }

        throw new ArgumentException(
            "Значение адреса не валидно, невозможно выполнить запрос на сервер",
            nameof(url));
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Значение имени не валидно, ожидается не пустая строка", nameof(name));
        }

        return Uri.EscapeDataString(name.Trim());
    }

    private static string ValidateVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Значение версии не валидно, ожидается не пустая строка", nameof(version));
        }

        return Uri.EscapeDataString(version.Trim());
    }

    private static void ValidateResponse(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new HttpRequestException("Пакет не найден в репозитории");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Код ошибки: {response.StatusCode}");
        }
    }
}