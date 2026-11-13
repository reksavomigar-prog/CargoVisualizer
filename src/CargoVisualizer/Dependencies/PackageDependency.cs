namespace CargoVisualizer.Dependencies;

/// <summary>
/// Представляет неизменяемую модель прямой зависимости пакета Cargo.
/// </summary>
public class PackageDependency
{
    /// <summary>
    /// Имя зависимого пакета.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Диапазон совместимых версий зависимого пакета (например, <c>^1.0.195</c>).
    /// </summary>
    public string VersionRequirement { get; }

    /// <summary>
    /// Тип зависимости (<c>normal</c>, <c>dev</c> или <c>build</c>).
    /// </summary>
    public string Kind { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="PackageDependency"/>.
    /// </summary>
    /// <param name="name">Имя зависимого пакета.</param>
    /// <param name="versionRequirement">Требование к диапазону версий пакета.</param>
    /// <param name="kind">Тип зависимости (<c>normal</c>, <c>dev</c> или <c>build</c>).</param>
    /// <exception cref="ArgumentException">
    /// Выбрасывается, если любой из параметров пуст или имеет недопустимое значение.
    /// </exception>
    public PackageDependency(
        string name,
        string versionRequirement,
        string kind)
    {
        Name = ValidateName(name);
        VersionRequirement = ValidateVersion(versionRequirement);
        Kind = ValidateKind(kind);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Значение имени не валидно, ожидается не пустая строка", nameof(name));
        }

        return name.Trim();
    }

    private static string ValidateVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Значение версии не валидно, ожидается не пустая строка", nameof(version));
        }

        return version.Trim();
    }

    private static string ValidateKind(string kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            throw new ArgumentException(
                "Значение типа зависимости не валидно, ожидается не пустая строка",
                nameof(kind));
        }

        var adjustedKind = kind.Trim().ToLowerInvariant();
        switch (adjustedKind)
        {
            case "normal":
                return "normal";
            case "dev":
                return "dev";
            case "build":
                return "build";
        }

        throw new ArgumentException(
            "Значение типа зависимости не валидно, допустимы исключительно значения normal/dev/build",
            nameof(kind));
    }

    /// <summary>
    /// Возвращает строковое представление зависимости в формате <c>Имя (Версия) [Тип]</c>.
    /// </summary>
    /// <returns>Отформатированная строка с информацией о зависимости.</returns>
    public override string ToString()
    {
        return $"{Name} ({VersionRequirement}) [{Kind}]";
    }
}