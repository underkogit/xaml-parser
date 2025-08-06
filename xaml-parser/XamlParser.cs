using xaml_parser.Native;

namespace xaml_parser;

public sealed class XamlParser : IDisposable
{
    private readonly List<XamlDocument> _documents = [];
    private bool _disposed;

    public XamlDocument? ParseDocument(string xamlContent)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        var element = NativeXamlParser.ParseXaml(xamlContent);
        if (element is null) return null;

        var document = new XamlDocument(element);
        _documents.Add(document);
        return document;
    }

    public XamlDocument? ParseDocumentFromStream(Stream stream)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        var xamlContent = reader.ReadToEnd();
        return ParseDocument(xamlContent);
    }

    public async Task<XamlDocument?> ParseDocumentFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        var xamlContent = await reader.ReadToEndAsync(cancellationToken);
        return ParseDocument(xamlContent);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _documents.ForEach(doc => doc.Dispose());
            _documents.Clear();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~XamlParser()
    {
        Dispose();
    }
}