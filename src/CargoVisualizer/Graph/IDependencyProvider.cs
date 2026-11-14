namespace CargoVisualizer.Graph;

/// <summary>
/// Определяет контракт поставщика прямых зависимостей для указанного пакета.
/// </summary>
public interface IDependencyProvider
{
    /// <summary>
    /// Асинхронно возвращает список имен прямых зависимостей указанного пакета.
    /// </summary>
    /// <param name="packageName">Имя анализируемого пакета.</param>
    /// <returns>Коллекция имен прямых зависимостей только для чтения.</returns>
    Task<IReadOnlyList<string>> GetDependenciesAsync(string packageName);
}