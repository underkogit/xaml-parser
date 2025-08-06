using System.Runtime.InteropServices;

namespace xaml_parser.Structures;


[StructLayout(LayoutKind.Sequential)]
public struct NativeXamlElement
{
    public nint Name;
    public nint Namespace;
    public nint Attributes;
    public nuint AttributesLen;
    public nint Children;
    public nuint ChildrenLen;
    public nint TextContent;
}
public record XamlElement(
    string Name, 
    string? Namespace, 
    Dictionary<string, string> Attributes, 
    List<XamlElement> Children, 
    string? TextContent
)
{
    public string? GetAttribute(string name) => 
        Attributes.GetValueOrDefault(name);

    public T? GetAttribute<T>(string name) where T : IConvertible
    {
        var value = GetAttribute(name);
        return value is not null ? (T)Convert.ChangeType(value, typeof(T)) : default;
    }

    public bool HasAttribute(string name) => 
        Attributes.ContainsKey(name);

    public IEnumerable<XamlElement> GetChildrenByName(string name) =>
        Children.Where(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public XamlElement? GetChildByName(string name) =>
        GetChildrenByName(name).FirstOrDefault();
}