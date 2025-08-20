using System.Text;

namespace xaml_parser;

/// <summary>
/// Специализированный парсер для работы с XAML файлами и директориями.
/// </summary>
/// <remarks>
/// Предоставляет высокоуровневые методы для парсинга файлов с различными кодировками,
/// асинхронной обработкой и batch-операциями для директорий.
/// </remarks>
public sealed class XamlFileParser : IDisposable
{
    #region Fields

    /// <summary>
    /// Внутренний экземпляр XamlParser для выполнения операций парсинга.
    /// </summary>
    private readonly XamlParser _parser = new();
    
    /// <summary>
    /// Флаг, указывающий на то, что объект был освобожден.
    /// </summary>
    private bool _disposed;

    #endregion

    #region Public Methods

    /// <summary>
    /// Парсит XAML файл с использованием UTF-8 кодировки.
    /// </summary>
    /// <param name="filePath">Путь к XAML файлу.</param>
    /// <returns>Распарсенный XAML документ или null, если парсинг не удался.</returns>
    /// <exception cref="ObjectDisposedException">Выбрасывается, если парсер уже освобожден.</exception>
    /// <exception cref="ArgumentNullException">Выбрасывается, если filePath равен null.</exception>
    /// <exception cref="FileNotFoundException">Выбрасывается, если файл не найден.</exception>
    /// <exception cref="InvalidOperationException">Выбрасывается при ошибках чтения файла.</exception>
    public XamlDocument? ParseFile(string filePath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(filePath);
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"XAML file not found: {filePath}");

        try
        {
            var xamlContent = File.ReadAllText(filePath, Encoding.UTF8);
            return _parser.ParseDocument(xamlContent);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to read XAML file: {filePath}", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException($"Access denied to XAML file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Асинхронно парсит XAML файл с использованием UTF-8 кодировки.
    /// </summary>
    /// <param name="filePath">Путь к XAML файлу.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Task с распарсенным XAML документом или null, если парсинг не удался.</returns>
    /// <exception cref="ObjectDisposedException">Выбрасывается, если парсер уже освобожден.</exception>
    /// <exception cref="ArgumentNullException">Выбрасывается, если filePath равен null.</exception>
    /// <exception cref="FileNotFoundException">Выбрасывается, если файл не найден.</exception>
    /// <exception cref="InvalidOperationException">Выбрасывается при ошибках чтения файла.</exception>
    public async Task<XamlDocument?> ParseFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(filePath);
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"XAML file not found: {filePath}");

        try
        {
            var xamlContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
            return _parser.ParseDocument(xamlContent);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to read XAML file: {filePath}", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException($"Access denied to XAML file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Парсит XAML файл с указанной кодировкой.
    /// </summary>
    /// <param name="filePath">Путь к XAML файлу.</param>
    /// <param name="encoding">Кодировка для чтения файла.</param>
    /// <returns>Распарсенный XAML документ или null, если парсинг не удался.</returns>
    /// <exception cref="ObjectDisposedException">Выбрасывается, если парсер уже освобожден.</exception>
    /// <exception cref="ArgumentNullException">Выбрасывается, если filePath или encoding равны null.</exception>
    /// <exception cref="FileNotFoundException">Выбрасывается, если файл не найден.</exception>
    /// <exception cref="InvalidOperationException">Выбрасывается при ошибках чтения файла.</exception>
    public XamlDocument? ParseFileWithEncoding(string filePath, Encoding encoding)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(encoding);
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"XAML file not found: {filePath}");

        try
        {
            var xamlContent = File.ReadAllText(filePath, encoding);
            return _parser.ParseDocument(xamlContent);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to read XAML file: {filePath}", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException($"Access denied to XAML file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Парсит все XAML файлы в указанной директории.
    /// </summary>
    /// <param name="directoryPath">Путь к директории с XAML файлами.</param>
    /// <param name="searchPattern">Шаблон поиска файлов (по умолчанию "*.xaml").</param>
    /// <returns>Генератор распарсенных XAML документов.</returns>
    /// <exception cref="ObjectDisposedException">Выбрасывается, если парсер уже освобожден.</exception>
    /// <exception cref="ArgumentNullException">Выбрасывается, если directoryPath или searchPattern равны null.</exception>
    /// <exception cref="DirectoryNotFoundException">Выбрасывается, если директория не найдена.</exception>
    /// <remarks>
    /// Обрабатывает только файлы в указанной директории (не рекурсивно).
    /// Файлы, которые не удалось распарсить, пропускаются без исключений.
    /// </remarks>
    public IEnumerable<XamlDocument> ParseDirectory(string directoryPath, string searchPattern = "*.xaml")
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(directoryPath);
        ArgumentNullException.ThrowIfNull(searchPattern);
        
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var xamlFiles = Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
        
        foreach (var file in xamlFiles)
        {
            XamlDocument? document = null;
            try
            {
                document = ParseFile(file);
            }
            catch
            {
                // Пропускаем файлы, которые не удалось распарсить
                continue;
            }
            
            if (document is not null)
                yield return document;
        }
    }

    /// <summary>
    /// Асинхронно парсит все XAML файлы в указанной директории.
    /// </summary>
    /// <param name="directoryPath">Путь к директории с XAML файлами.</param>
    /// <param name="searchPattern">Шаблон поиска файлов (по умолчанию "*.xaml").</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Асинхронный генератор распарсенных XAML документов.</returns>
    /// <exception cref="ObjectDisposedException">Выбрасывается, если парсер уже освобожден.</exception>
    /// <exception cref="ArgumentNullException">Выбрасывается, если directoryPath или searchPattern равны null.</exception>
    /// <exception cref="DirectoryNotFoundException">Выбрасывается, если директория не найдена.</exception>
    /// <remarks>
    /// Обрабатывает только файлы в указанной директории (не рекурсивно).
    /// Файлы, которые не удалось распарсить, пропускаются без исключений.
    /// </remarks>
    public async IAsyncEnumerable<XamlDocument> ParseDirectoryAsync(
        string directoryPath, 
        string searchPattern = "*.xaml",
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(directoryPath);
        ArgumentNullException.ThrowIfNull(searchPattern);
        
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var xamlFiles = Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
        
        foreach (var file in xamlFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            XamlDocument? document = null;
            try
            {
                document = await ParseFileAsync(file, cancellationToken);
            }
            catch
            {
                // Пропускаем файлы, которые не удалось распарсить
                continue;
            }
            
            if (document is not null)
                yield return document;
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
            // Освобождаем внутренний парсер
            _parser.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Финализатор для освобождения ресурсов в случае, если Dispose не был вызван.
    /// </summary>
    ~XamlFileParser()
    {
        Dispose();
    }

    #endregion
}