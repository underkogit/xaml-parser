# XAML Parser

Высокопроизводительная библиотека для парсинга XAML файлов с использованием гибридной архитектуры C#/.NET и Rust.

## Описание

XAML Parser - это кроссплатформенная библиотека, предназначенная для эффективного парсинга и анализа XAML документов. Библиотека использует нативную Rust-библиотеку для высокой производительности парсинга с удобным C# API для интеграции в .NET приложения.

## Возможности

- ✅ Парсинг XAML файлов из строк, потоков и файлов
- ✅ Поиск элементов по имени, атрибутам, ID и пользовательским предикатам
- ✅ Типизированное получение атрибутов с автоматической конвертацией
- ✅ Поддержка асинхронных операций для всех файловых операций
- ✅ Batch-обработка директорий с XAML файлами
- ✅ Автоматическое управление памятью через IDisposable
- ✅ Поддержка различных кодировок текста
- ✅ Высокая производительность благодаря нативному Rust backend
- ✅ Подробная документация и комментарии в коде
- ✅ Демонстрационное консольное приложение с примерами

## Структура проекта

```
xaml-parser/
├── xaml-parser/              # Основная C# библиотека
│   ├── XamlParser.cs        # Главный класс парсера
│   ├── XamlDocument.cs      # Документ XAML с методами поиска
│   ├── XamlFileParser.cs    # Парсер файлов
│   ├── Native/              # Интеграция с нативной библиотекой
│   └── Structures/          # Структуры данных
├── xaml-parser-native/      # Нативная Rust библиотека
├── XamlParserConsoleApp/    # Пример использования
└── README.md
```

## Быстрый старт

### Установка

1. Клонируйте репозиторий:
```bash
git clone <repository-url>
cd xaml-parser
```

2. Постройте решение:
```bash
dotnet build
```

### Основное использование

#### Парсинг XAML из строки

```csharp
using xaml_parser;

// Использование основного парсера с автоматическим управлением ресурсами
using var parser = new XamlParser();
var document = parser.ParseDocument(@"
    <Window x:Class=""MainWindow""
            xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
            Title=""Sample Window"" Width=""800"" Height=""600"">
        <Grid>
            <Button x:Name=""SaveButton"" Content=""Save"" Width=""100"" Margin=""10""/>
            <TextBox x:Name=""InputBox"" Text=""Enter text..."" Width=""200""/>
        </Grid>
    </Window>
");

if (document != null)
{
    // Поиск элементов по ID
    var button = document.FindElementById("SaveButton");
    if (button != null)
    {
        Console.WriteLine($"Button Content: {button.GetAttribute("Content")}");
        Console.WriteLine($"Button Width: {button.GetAttribute<int>("Width")}");
    }

    // Статистика документа
    Console.WriteLine($"Корневой элемент: {document.RootElement}");
    Console.WriteLine($"Всего кнопок: {document.FindElementsByName("Button").Count()}");
}
```

#### Парсинг файла

```csharp
using xaml_parser;

using var fileParser = new XamlFileParser();
var document = fileParser.ParseFile("path/to/your/file.xaml");

// Поиск элементов
var buttons = document?.FindElementsByName("Button");
var elementsWithId = document?.FindElementsByAttribute("x:Name");
```

#### Асинхронная обработка

```csharp
using xaml_parser;

using var fileParser = new XamlFileParser();
var document = await fileParser.ParseFileAsync("path/to/file.xaml");

// Обработка директории
await foreach (var doc in fileParser.ParseDirectoryAsync("path/to/xaml/files"))
{
    Console.WriteLine($"Root element: {doc.RootElement.Name}");
}
```

## API Reference

### XamlParser

Основной класс для парсинга XAML из строк и потоков.

```csharp
public sealed class XamlParser : IDisposable
{
    XamlDocument? ParseDocument(string xamlContent)
    XamlDocument? ParseDocumentFromStream(Stream stream)
    Task<XamlDocument?> ParseDocumentFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
}
```

### XamlFileParser

Специализированный класс для работы с файлами.

```csharp
public sealed class XamlFileParser : IDisposable
{
    XamlDocument? ParseFile(string filePath)
    Task<XamlDocument?> ParseFileAsync(string filePath, CancellationToken cancellationToken = default)
    XamlDocument? ParseFileWithEncoding(string filePath, Encoding encoding)
    IEnumerable<XamlDocument> ParseDirectory(string directoryPath, string searchPattern = "*.xaml")
    IAsyncEnumerable<XamlDocument> ParseDirectoryAsync(string directoryPath, string searchPattern = "*.xaml", CancellationToken cancellationToken = default)
}
```

### XamlDocument

Представляет распарсенный XAML документ с методами поиска.

```csharp
public sealed class XamlDocument : IDisposable
{
    XamlElement RootElement { get; }
    
    // Методы поиска
    IEnumerable<XamlElement> FindElementsByName(string name)
    IEnumerable<XamlElement> FindElementsByAttribute(string attributeName, string? attributeValue = null)
    XamlElement? FindElementByName(string name)
    XamlElement? FindElementById(string id)
    IEnumerable<XamlElement> FindElementsByType(string typeName)
}
```

### XamlElement

Представляет элемент в XAML документе.

```csharp
public record XamlElement(
    string Name, 
    string? Namespace, 
    Dictionary<string, string> Attributes, 
    List<XamlElement> Children, 
    string? TextContent
)
{
    string? GetAttribute(string name)
    T? GetAttribute<T>(string name) where T : IConvertible
    bool HasAttribute(string name)
    IEnumerable<XamlElement> GetChildrenByName(string name)
    XamlElement? GetChildByName(string name)
}
```

## Примеры использования

### Поиск элементов

```csharp
using var parser = new XamlParser();
var document = parser.ParseDocument(xamlContent);

if (document != null)
{
    // Поиск по имени элемента (регистронезависимый)
    var buttons = document.FindElementsByName("Button");
    Console.WriteLine($"Найдено кнопок: {buttons.Count()}");

    // Поиск по атрибуту (с опциональным значением)
    var namedElements = document.FindElementsByAttribute("x:Name");
    var elementsWithSpecificMargin = document.FindElementsByAttribute("Margin", "10");

    // Поиск по ID (поддерживает x:Name и Name)
    var specificButton = document.FindElementById("SaveButton");
    if (specificButton != null)
    {
        Console.WriteLine($"Найдена кнопка: {specificButton}");
    }

    // Поиск по типу (эквивалентно поиску по имени для XAML)
    var textBlocks = document.FindElementsByType("TextBlock");

    // Поиск с использованием предиката
    var wideElements = document.FindElements(element => 
    {
        var widthStr = element.GetAttribute("Width");
        return double.TryParse(widthStr, out var width) && width > 200;
    });

    Console.WriteLine($"Элементов шире 200px: {wideElements.Count()}");
}
```

### Работа с атрибутами

```csharp
var element = document?.FindElementById("MyButton");
if (element != null)
{
    // Получить атрибут как строку (возвращает null, если атрибут не найден)
    var content = element.GetAttribute("Content");
    Console.WriteLine($"Содержимое: {content ?? "Не задано"}");
    
    // Получить атрибут с автоматической конвертацией типа
    var width = element.GetAttribute<double>("Width"); // Возвращает 0.0 если не найден
    var margin = element.GetAttribute<int>("Margin");   // Поддерживает любые IConvertible типы
    
    // Проверить наличие атрибута перед использованием
    if (element.HasAttribute("IsEnabled"))
    {
        var isEnabled = element.GetAttribute<bool>("IsEnabled");
        Console.WriteLine($"Элемент включен: {isEnabled}");
    }
    
    // Безопасная работа с опциональными атрибутами
    var opacity = element.GetAttribute<double?>("Opacity");
    if (opacity.HasValue)
    {
        Console.WriteLine($"Прозрачность: {opacity.Value:F2}");
    }
    
    // Работа с комплексными атрибутами
    var style = element.GetAttribute("Style");
    var transform = element.GetAttribute("RenderTransform");
    
    // Отладочная информация об элементе
    Console.WriteLine($"Элемент: {element}"); // Использует переопределенный ToString()
}
```

### Обход дерева элементов

```csharp
// Простой обход с выводом структуры
void PrintElementTree(XamlElement element, int indent = 0)
{
    var spaces = new string(' ', indent * 2);
    var elementInfo = element.Name;
    
    // Добавляем ID если есть
    var id = element.GetAttribute("x:Name") ?? element.GetAttribute("Name");
    if (!string.IsNullOrEmpty(id))
        elementInfo += $" (ID: {id})";
    
    Console.WriteLine($"{spaces}📄 {elementInfo}");
    
    // Показываем ключевые атрибуты
    var importantAttrs = element.Attributes
        .Where(kvp => new[] { "Content", "Text", "Width", "Height", "Margin" }
            .Contains(kvp.Key))
        .Take(3);
        
    foreach (var attr in importantAttrs)
    {
        Console.WriteLine($"{spaces}   @{attr.Key} = \"{attr.Value}\"");
    }
    
    // Показываем текстовое содержимое
    if (!string.IsNullOrEmpty(element.TextContent?.Trim()))
    {
        var text = element.TextContent.Trim();
        if (text.Length > 50)
            text = text.Substring(0, 47) + "...";
        Console.WriteLine($"{spaces}   📝 \"{text}\"");
    }
    
    // Рекурсивно обходим дочерние элементы
    foreach (var child in element.Children)
    {
        PrintElementTree(child, indent + 1);
    }
}

// Подсчет статистики
int CountElements(XamlElement element)
{
    return 1 + element.Children.Sum(CountElements);
}

// Использование
if (document?.RootElement != null)
{
    Console.WriteLine($"Структура документа (всего элементов: {CountElements(document.RootElement)}):");
    PrintElementTree(document.RootElement);
    
    // Использование встроенного ToString() для отладки
    Console.WriteLine($"\nКорневой элемент: {document.RootElement}");
}
```

## Обработка ошибок

```csharp
using var fileParser = new XamlFileParser();

try
{
    var document = fileParser.ParseFile("path/to/file.xaml");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Parse error: {ex.Message}");
}
```

## Производительность

- Использование нативной Rust библиотеки для высокой скорости парсинга
- Эффективное управление памятью
- Асинхронные операции для неблокирующего I/O
- Batch-обработка множественных файлов

## Системные требования

- .NET 6.0 или выше
- Поддержка нативных библиотек (Windows, Linux, macOS)

## Лицензия

См. файл [LICENSE.txt](LICENSE.txt) для подробностей.

## Разработка

### Сборка проекта

```bash
# Сборка всего решения
dotnet build

# Сборка в Release режиме
dotnet build -c Release

# Запуск тестов (если есть)
dotnet test
```

### Структура нативной части

Нативная часть реализована на Rust и находится в директории `xaml-parser-native/`. Для её сборки требуется установленный Rust toolchain.

### Использование демонстрационного приложения

Проект включает консольное демонстрационное приложение `XamlParserConsoleApp`, которое показывает все основные возможности библиотеки:

```bash
# Запуск демонстрации с встроенным примером
dotnet run --project XamlParserConsoleApp

# Запуск с парсингом конкретного файла
dotnet run --project XamlParserConsoleApp -- "path/to/your/file.xaml"
```

Демонстрационное приложение покажет:
- Парсинг XAML из строки с подробным выводом
- Различные способы поиска элементов (по ID, имени, атрибутам, предикатам)
- Обход и визуализацию структуры документа
- Работу с атрибутами и их типизированное получение
- Парсинг файлов (синхронный и асинхронный)
- Примеры обработки ошибок

### Архитектурные особенности

- **Гибридная архитектура**: C# обеспечивает удобный API, Rust - высокую производительность парсинга
- **Безопасное управление памятью**: Автоматическое освобождение нативных ресурсов через IDisposable
- **Типобезопасность**: Строгая типизация с поддержкой nullable reference types
- **Производительность**: Lazy evaluation для поисковых операций, эффективный маршалинг данных

## Поддержка и контрибуции

Если у вас есть вопросы, предложения или вы нашли баг, пожалуйста, создайте issue в репозитории проекта.