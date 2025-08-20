using System.Runtime.InteropServices;

namespace xaml_parser.Structures;

/// <summary>
/// Нативная структура для представления атрибута XAML элемента.
/// </summary>
/// <remarks>
/// Используется для маршалинга данных между C# и Rust.
/// Оба поля являются указателями на UTF-8 строки в неуправляемой памяти.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct NativeXamlAttribute
{
    /// <summary>
    /// Указатель на UTF-8 строку с именем атрибута.
    /// </summary>
    public nint Key;
    
    /// <summary>
    /// Указатель на UTF-8 строку со значением атрибута.
    /// </summary>
    public nint Value;
}