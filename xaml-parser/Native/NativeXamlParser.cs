using System.Runtime.InteropServices;
using xaml_parser.Structures;

namespace xaml_parser.Native;

public static class NativeXamlParser
{
    #region Native Methods

    [DllImport(Interop.NativeLib, EntryPoint = "parse_xaml", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ParseXamlNative([MarshalAs(UnmanagedType.LPStr)] string xml, out nint result);

    [DllImport(Interop.NativeLib, EntryPoint = "free_xaml_element", CallingConvention = CallingConvention.Cdecl)]
    private static extern int FreeXamlElementNative(nint element);

    #endregion

    public static XamlElement? ParseXaml(string xml)
    {
        using var wrapper = new XamlElementWrapper(xml);
        return wrapper.Element;
    }

    internal static XamlElement MarshalXamlElement(nint ptr)
    {
        var native = Marshal.PtrToStructure<NativeXamlElement>(ptr);
        var name = Marshal.PtrToStringUTF8(native.Name) ?? string.Empty;
        var ns = native.Namespace != 0 ? Marshal.PtrToStringUTF8(native.Namespace) : null;
        var textContent = native.TextContent != 0 ? Marshal.PtrToStringUTF8(native.TextContent) : null;

        var attributes = new Dictionary<string, string>();
        if (native.Attributes != 0 && native.AttributesLen > 0)
        {
            for (int i = 0; i < (int)native.AttributesLen; i++)
            {
                var attrPtr = native.Attributes + i * Marshal.SizeOf<NativeXamlAttribute>();
                var attr = Marshal.PtrToStructure<NativeXamlAttribute>(attrPtr);
                var key = Marshal.PtrToStringUTF8(attr.Key) ?? string.Empty;
                var value = Marshal.PtrToStringUTF8(attr.Value) ?? string.Empty;
                attributes[key] = value;
            }
        }

        var children = new List<XamlElement>();
        if (native.Children != 0 && native.ChildrenLen > 0)
        {
            for (int i = 0; i < (int)native.ChildrenLen; i++)
            {
                var childPtr = Marshal.ReadIntPtr(native.Children + i * nint.Size);
                if (childPtr != 0)
                {
                    children.Add(MarshalXamlElement(childPtr));
                }
            }
        }

        return new XamlElement(name, ns, attributes, children, textContent);
    }

    internal sealed class XamlElementWrapper : IDisposable
    {
        private nint _elementPtr;
        private bool _disposed;

        public XamlElement? Element { get; }

        public XamlElementWrapper(string xml)
        {
            var result = ParseXamlNative(xml, out _elementPtr);
            if (result == 0 && _elementPtr != 0)
            {
                Element = MarshalXamlElement(_elementPtr);
            }
        }

        public void Dispose()
        {
            if (!_disposed && _elementPtr != 0)
            {
                FreeXamlElementNative(_elementPtr);
                _elementPtr = 0;
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Финализатор для освобождения ресурсов в случае, если Dispose не был вызван.
        /// </summary>
        ~XamlElementWrapper()
        {
            Dispose();
        }

        
    }

    
}