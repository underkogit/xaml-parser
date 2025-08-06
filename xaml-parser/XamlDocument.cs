using xaml_parser.Structures;

namespace xaml_parser;

public sealed class XamlDocument : IDisposable
{
    private bool _disposed;

    public XamlElement RootElement { get; }

    internal XamlDocument(XamlElement rootElement)
    {
        RootElement = rootElement;
    }

    public IEnumerable<XamlElement> FindElementsByName(string name) =>
        FindElementsRecursive(RootElement, e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<XamlElement> FindElementsByAttribute(string attributeName, string? attributeValue = null) =>
        FindElementsRecursive(RootElement, e => 
            e.Attributes.ContainsKey(attributeName) && 
            (attributeValue is null || e.Attributes[attributeName] == attributeValue));

    public XamlElement? FindElementByName(string name) =>
        FindElementsByName(name).FirstOrDefault();

    public XamlElement? FindElementById(string id) =>
        FindElementsByAttribute("x:Name", id).FirstOrDefault() ?? 
        FindElementsByAttribute("Name", id).FirstOrDefault();
    
 
    public IEnumerable<XamlElement> FindElementsByType(string typeName) =>
        FindElementsByName(typeName);

    private static IEnumerable<XamlElement> FindElementsRecursive(XamlElement element, Func<XamlElement, bool> predicate)
    {
        if (predicate(element))
            yield return element;

        foreach (var child in element.Children)
        foreach (var result in FindElementsRecursive(child, predicate))
            yield return result;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~XamlDocument()
    {
        Dispose();
    }
}