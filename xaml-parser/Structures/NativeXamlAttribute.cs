using System.Runtime.InteropServices;

namespace xaml_parser.Structures;

[StructLayout(LayoutKind.Sequential)]
public struct NativeXamlAttribute
{
    public nint Key;
    public nint Value;
}