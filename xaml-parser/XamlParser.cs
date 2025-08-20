using xaml_parser.Native;

namespace xaml_parser;

/// <summary>
/// Основной класс для парсинга XAML документов из различных источников.
/// Предоставляет высокоуровневый API для работы с XAML контентом.
/// </summary>
/// <remarks>
/// Использует нативную Rust библиотеку для высокопроизводительного парсинга.
/// Реализует паттерн IDisposable для корректного управления ресурсами.
/// </remarks>
public sealed class XamlParser : IDisposable
{
    #region Fields

    /// <summary>
    /// Коллекция всех созданных документов для корректной очистки ресурсов.
    /// </summary>
    private readonly List<XamlDocument> _documents = [];
    
    /// <summary>
    /// Флаг, указывающий на то, что объект был освобожден.
    /// </summary>
    private bool _disposed;

    #endregion

    #region Public Methods

    /// <summary>
    /// Парсит XAML документ из строки.
    /// </summary>
    /// <param name="xamlContent">XAML контент в виде строки.</param>
    /// <returns>Распарсенный XAML документ или null, если парсинг не удался.</returns>
    /// <exception cref="ObjectDisposedException">Выбрасывается, если парсер уже освобожден.</exception>
    /// <exception cref="ArgumentNullException">Выбрасывается, если xamlContent равен null.</exception>
    public XamlDocument? ParseDocument(string xamlContent)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(xamlContent);
        
        // Используем нативную библиотеку для парсинга
        var element = NativeXamlParser.ParseXaml(xamlContent);
        if (element is null) 
            return null;

        // Создаем документ и добавляем в коллекцию для отслеживания
        var document = new XamlDocument(element);
        _documents.Add(document);
        return document;
    }

    /// <summary>
    /// Парсит XAML документ из потока данных.
    /// </summary>
    /// <param name="stream">Поток с XAML контентом.</param>
    /// <returns>Распарсенный XAML документ или null, если парсинг не удался.</returns>
    /// <exception cref="ObjectDisposedException">Выбрасывается, если парсер уже освобожден.</exception>
    /// <exception cref="ArgumentNullException">Выбрасывается, если stream равен null.</exception>
    public XamlDocument? ParseDocumentFromStream(Stream stream)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(stream);
        
        // Читаем весь поток в строку с UTF-8 кодировкой
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        var xamlContent = reader.ReadToEnd();
        return ParseDocument(xamlContent);
    }

    /// <summary>
    /// Асинхронно парсит XAML документ из потока данных.
    /// </summary>
    /// <param name="stream">Поток с XAML контентом.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Task с распарсенным XAML документом или null, если парсинг не удался.</returns>
    /// <exception cref="ObjectDisposedException">Выбрасывается, если парсер уже освобожден.</exception>
    /// <exception cref="ArgumentNullException">Выбрасывается, если stream равен null.</exception>
    public async Task<XamlDocument?> ParseDocumentFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(stream);
        
        // Асинхронно читаем весь поток в строку с UTF-8 кодировкой
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        var xamlContent = await reader.ReadToEndAsync(cancellationToken);
        return ParseDocument(xamlContent);
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
            // Освобождаем все созданные документы
            _documents.ForEach(doc => doc.Dispose());
            _documents.Clear();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Финализатор для освобождения ресурсов в случае, если Dispose не был вызван.
    /// </summary>
    ~XamlParser()
    {
        Dispose();
    }

    #endregion
}