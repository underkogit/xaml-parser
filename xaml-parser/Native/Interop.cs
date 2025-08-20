namespace xaml_parser.Native;

/// <summary>
/// Содержит константы для взаимодействия с нативными библиотеками.
/// </summary>
/// <remarks>
/// Определяет имена нативных библиотек для различных операционных систем.
/// В текущей версии поддерживается только Windows (DLL), но может быть расширен
/// для поддержки Linux (SO) и macOS (DYLIB).
/// </remarks>
public static class Interop
{
    /// <summary>
    /// Имя нативной библиотеки для Windows.
    /// </summary>
    /// <remarks>
    /// Для кроссплатформенной поддержки можно использовать условную компиляцию
    /// или определение имени библиотеки во время выполнения.
    /// </remarks>
    public const string NativeLib = "xaml_parser_native.dll";
}