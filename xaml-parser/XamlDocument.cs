using xaml_parser.Structures;

namespace xaml_parser;

/// <summary>
/// Представляет распарсенный XAML документ с возможностями поиска и навигации по элементам.
/// </summary>
/// <remarks>
/// Предоставляет высокоуровневые методы для поиска элементов по различным критериям.
/// Реализует паттерн IDisposable для корректного управления ресурсами.
/// </remarks>
public sealed class XamlDocument : IDisposable
{
    #region Fields

    /// <summary>
    /// Флаг, указывающий на то, что объект был освобожден.
    /// </summary>
    private bool _disposed;

    #endregion

    #region Properties

    /// <summary>
    /// Корневой элемент XAML документа.
    /// </summary>
    public XamlElement RootElement { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Инициализирует новый экземпляр XamlDocument с указанным корневым элементом.
    /// </summary>
    /// <param name="rootElement">Корневой элемент документа.</param>
    internal XamlDocument(XamlElement rootElement)
    {
        RootElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
    }

    #endregion

    #region Public Search Methods

    /// <summary>
    /// Находит все элементы с указанным именем (тегом).
    /// </summary>
    /// <param name="name">Имя элемента для поиска (регистронезависимый).</param>
    /// <returns>Коллекция найденных элементов.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если name равен null.</exception>
    public IEnumerable<XamlElement> FindElementsByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return FindElementsRecursive(RootElement, e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Находит все элементы, содержащие указанный атрибут.
    /// </summary>
    /// <param name="attributeName">Имя атрибута для поиска.</param>
    /// <param name="attributeValue">Значение атрибута (опционально). Если null, ищет элементы с любым значением атрибута.</param>
    /// <returns>Коллекция найденных элементов.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если attributeName равен null.</exception>
    public IEnumerable<XamlElement> FindElementsByAttribute(string attributeName, string? attributeValue = null)
    {
        ArgumentNullException.ThrowIfNull(attributeName);
        return FindElementsRecursive(RootElement, e => 
            e.Attributes.ContainsKey(attributeName) && 
            (attributeValue is null || e.Attributes[attributeName] == attributeValue));
    }

    /// <summary>
    /// Находит первый элемент с указанным именем.
    /// </summary>
    /// <param name="name">Имя элемента для поиска.</param>
    /// <returns>Найденный элемент или null, если элемент не найден.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если name равен null.</exception>
    public XamlElement? FindElementByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return FindElementsByName(name).FirstOrDefault();
    }

    /// <summary>
    /// Находит элемент по его идентификатору (x:Name или Name атрибут).
    /// </summary>
    /// <param name="id">Идентификатор элемента.</param>
    /// <returns>Найденный элемент или null, если элемент не найден.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если id равен null.</exception>
    public XamlElement? FindElementById(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        // Сначала ищем по x:Name, затем по Name
        return FindElementsByAttribute("x:Name", id).FirstOrDefault() ?? 
               FindElementsByAttribute("Name", id).FirstOrDefault();
    }

    /// <summary>
    /// Находит все элементы указанного типа.
    /// </summary>
    /// <param name="typeName">Имя типа элемента.</param>
    /// <returns>Коллекция найденных элементов.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если typeName равен null.</exception>
    /// <remarks>
    /// Этот метод эквивалентен FindElementsByName для XAML элементов.
    /// </remarks>
    public IEnumerable<XamlElement> FindElementsByType(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);
        return FindElementsByName(typeName);
    }

    /// <summary>
    /// Находит все элементы, удовлетворяющие заданному предикату.
    /// </summary>
    /// <param name="predicate">Функция-предикат для фильтрации элементов.</param>
    /// <returns>Коллекция найденных элементов.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если predicate равен null.</exception>
    public IEnumerable<XamlElement> FindElements(Func<XamlElement, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return FindElementsRecursive(RootElement, predicate);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Рекурсивно обходит дерево элементов и возвращает элементы, соответствующие предикату.
    /// </summary>
    /// <param name="element">Элемент, с которого начинается поиск.</param>
    /// <param name="predicate">Функция-предикат для проверки элементов.</param>
    /// <returns>Генератор найденных элементов.</returns>
    private static IEnumerable<XamlElement> FindElementsRecursive(XamlElement element, Func<XamlElement, bool> predicate)
    {
        // Проверяем текущий элемент
        if (predicate(element))
            yield return element;

        // Рекурсивно обходим дочерние элементы
        foreach (var child in element.Children)
        {
            foreach (var result in FindElementsRecursive(child, predicate))
            {
                yield return result;
            }
        }
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Освобождает все управляемые ресурсы.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // В текущей реализации особых ресурсов для освобождения нет,
            // но паттерн готов для будущих расширений
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Финализатор для освобождения ресурсов в случае, если Dispose не был вызван.
    /// </summary>
    ~XamlDocument()
    {
        Dispose();
    }

    #endregion
}