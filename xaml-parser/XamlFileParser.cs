namespace xaml_parser;

public sealed class XamlFileParser : IDisposable
{
    private readonly XamlParser _parser = new();
    private bool _disposed;

    public XamlDocument? ParseFile(string filePath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"XAML file not found: {filePath}");

        try
        {
            var xamlContent = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
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

    public async Task<XamlDocument?> ParseFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"XAML file not found: {filePath}");

        try
        {
            var xamlContent = await File.ReadAllTextAsync(filePath, System.Text.Encoding.UTF8, cancellationToken);
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

    public XamlDocument? ParseFileWithEncoding(string filePath, System.Text.Encoding encoding)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
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

    public IEnumerable<XamlDocument> ParseDirectory(string directoryPath, string searchPattern = "*.xaml")
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var xamlFiles = Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
        
        foreach (var file in xamlFiles)
        {
            var document = ParseFile(file);
            if (document is not null)
                yield return document;
        }
    }

    public async IAsyncEnumerable<XamlDocument> ParseDirectoryAsync(
        string directoryPath, 
        string searchPattern = "*.xaml",
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var xamlFiles = Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
        
        foreach (var file in xamlFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = await ParseFileAsync(file, cancellationToken);
            if (document is not null)
                yield return document;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _parser.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~XamlFileParser()
    {
        Dispose();
    }
}