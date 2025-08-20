using System.Text;
using xaml_parser;
using xaml_parser.Structures;

namespace XamlParserConsoleApp;

/// <summary>
/// Демонстрационное консольное приложение для библиотеки XAML Parser.
/// Показывает основные возможности парсинга и поиска элементов в XAML документах.
/// </summary>
internal static class Program
{
    #region Sample XAML Content

    /// <summary>
    /// Пример XAML контента для демонстрации возможностей парсера.
    /// </summary>
    private const string SampleXaml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
        Title=""Sample Window"" Width=""800"" Height=""600"">
    <Grid Name=""MainGrid"">
        <Grid.RowDefinitions>
            <RowDefinition Height=""Auto""/>
            <RowDefinition Height=""*""/>
            <RowDefinition Height=""Auto""/>
        </Grid.RowDefinitions>
        
        <TextBlock Grid.Row=""0"" x:Name=""HeaderText"" 
                   Text=""XAML Parser Demo"" 
                   FontSize=""20"" 
                   HorizontalAlignment=""Center""
                   Margin=""10""/>
        
        <StackPanel Grid.Row=""1"" Orientation=""Vertical"" Margin=""20"">
            <Button x:Name=""SaveButton"" Content=""Save"" Width=""100"" Margin=""5""/>
            <Button x:Name=""LoadButton"" Content=""Load"" Width=""100"" Margin=""5""/>
            <Button x:Name=""ExitButton"" Content=""Exit"" Width=""100"" Margin=""5""/>
            
            <TextBox x:Name=""InputTextBox"" 
                     Text=""Enter some text..."" 
                     Width=""200"" 
                     Height=""30"" 
                     Margin=""5""/>
        </StackPanel>
        
        <StatusBar Grid.Row=""2"">
            <StatusBarItem>
                <TextBlock x:Name=""StatusText"" Text=""Ready""/>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>";

    #endregion

    #region Main Method

    /// <summary>
    /// Главная точка входа в приложение.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    public static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== XAML Parser Console Demo ===\n");

            // Демонстрация парсинга из строки
            await DemonstrateStringParsing();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // Демонстрация работы с файлами (если есть аргументы командной строки)
            if (args.Length > 0)
            {
                await DemonstrateFileParsing(args[0]);
            }
            else
            {
                Console.WriteLine("Для демонстрации парсинга файлов передайте путь к XAML файлу в качестве аргумента.");
                Console.WriteLine("Пример: XamlParserConsoleApp.exe \"path/to/your/file.xaml\"");
            }
            
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("Демонстрация завершена. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
            Console.WriteLine($"Детали: {ex}");
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }

    #endregion

    #region Demonstration Methods

    /// <summary>
    /// Демонстрирует парсинг XAML из строки и различные способы поиска элементов.
    /// </summary>
    private static async Task DemonstrateStringParsing()
    {
        Console.WriteLine("1. Демонстрация парсинга из строки");
        Console.WriteLine(new string('-', 40));

        // Парсинг из строки
        using var parser = new XamlParser();
        var document = parser.ParseDocument(SampleXaml);

        if (document == null)
        {
            Console.WriteLine("❌ Не удалось распарсить XAML документ.");
            return;
        }

        Console.WriteLine("✅ XAML документ успешно распарсен!");
        Console.WriteLine($"   Корневой элемент: {document.RootElement}");

        // Демонстрация поиска элементов
        await DemonstrateElementSearch(document);

        // Демонстрация обхода дерева
        DemonstrateTreeTraversal(document);
    }

    /// <summary>
    /// Демонстрирует парсинг XAML файла.
    /// </summary>
    /// <param name="filePath">Путь к XAML файлу.</param>
    private static async Task DemonstrateFileParsing(string filePath)
    {
        Console.WriteLine("2. Демонстрация парсинга файла");
        Console.WriteLine(new string('-', 40));
        Console.WriteLine($"Файл: {filePath}");

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"❌ Файл не найден: {filePath}");
            return;
        }

        try
        {
            using var fileParser = new XamlFileParser();
            
            // Синхронный парсинг
            Console.WriteLine("📄 Синхронный парсинг файла...");
            var document = fileParser.ParseFile(filePath);
            
            if (document != null)
            {
                Console.WriteLine("✅ Файл успешно распарсен!");
                Console.WriteLine($"   Корневой элемент: {document.RootElement}");
                
                // Показываем основную статистику
                var elementCount = CountElements(document.RootElement);
                Console.WriteLine($"   Всего элементов: {elementCount}");
            }
            else
            {
                Console.WriteLine("❌ Не удалось распарсить файл.");
                return;
            }

            // Асинхронный парсинг
            Console.WriteLine("\n📄 Асинхронный парсинг того же файла...");
            var asyncDocument = await fileParser.ParseFileAsync(filePath);
            
            if (asyncDocument != null)
            {
                Console.WriteLine("✅ Асинхронный парсинг успешно завершен!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка при парсинге файла: {ex.Message}");
        }
    }

    /// <summary>
    /// Демонстрирует различные способы поиска элементов в документе.
    /// </summary>
    /// <param name="document">XAML документ для поиска.</param>
    private static async Task DemonstrateElementSearch(XamlDocument document)
    {
        Console.WriteLine("\n🔍 Демонстрация поиска элементов:");

        // Поиск по ID
        Console.WriteLine("\n   Поиск по ID:");
        var saveButton = document.FindElementById("SaveButton");
        if (saveButton != null)
        {
            Console.WriteLine($"   ✅ Найден элемент с ID 'SaveButton': {saveButton}");
            Console.WriteLine($"      Content: {saveButton.GetAttribute("Content")}");
            Console.WriteLine($"      Width: {saveButton.GetAttribute("Width")}");
        }

        var headerText = document.FindElementById("HeaderText");
        if (headerText != null)
        {
            Console.WriteLine($"   ✅ Найден элемент с ID 'HeaderText': {headerText}");
            Console.WriteLine($"      Text: {headerText.GetAttribute("Text")}");
            Console.WriteLine($"      FontSize: {headerText.GetAttribute("FontSize")}");
        }

        // Поиск по имени тега
        Console.WriteLine("\n   Поиск по имени элемента:");
        var buttons = document.FindElementsByName("Button").ToList();
        Console.WriteLine($"   ✅ Найдено кнопок: {buttons.Count}");
        foreach (var button in buttons)
        {
            var content = button.GetAttribute("Content") ?? "Без содержимого";
            var id = button.GetAttribute("x:Name") ?? button.GetAttribute("Name") ?? "Без ID";
            Console.WriteLine($"      - {content} (ID: {id})");
        }

        // Поиск по атрибуту
        Console.WriteLine("\n   Поиск по атрибуту 'Margin':");
        var elementsWithMargin = document.FindElementsByAttribute("Margin").ToList();
        Console.WriteLine($"   ✅ Найдено элементов с атрибутом 'Margin': {elementsWithMargin.Count}");
        foreach (var element in elementsWithMargin.Take(3)) // Показываем только первые 3
        {
            Console.WriteLine($"      - {element.Name}: Margin='{element.GetAttribute("Margin")}'");
        }

        // Поиск с помощью предиката
        Console.WriteLine("\n   Поиск элементов с шириной больше 150:");
        var wideElements = document.FindElements(e => 
        {
            var widthStr = e.GetAttribute("Width");
            if (double.TryParse(widthStr, out var width))
                return width > 150;
            return false;
        }).ToList();
        
        Console.WriteLine($"   ✅ Найдено широких элементов: {wideElements.Count}");
        foreach (var element in wideElements)
        {
            Console.WriteLine($"      - {element.Name}: Width={element.GetAttribute("Width")}");
        }
    }

    /// <summary>
    /// Демонстрирует обход дерева элементов и вывод структуры.
    /// </summary>
    /// <param name="document">XAML документ.</param>
    private static void DemonstrateTreeTraversal(XamlDocument document)
    {
        Console.WriteLine("\n🌳 Структура документа:");
        PrintElementTree(document.RootElement, 0);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Рекурсивно выводит дерево элементов в консоль.
    /// </summary>
    /// <param name="element">Элемент для вывода.</param>
    /// <param name="indent">Уровень отступа.</param>
    private static void PrintElementTree(XamlElement element, int indent)
    {
        var indentStr = new string(' ', indent * 2);
        var elementInfo = $"{element.Name}";
        
        // Добавляем информацию об ID, если есть
        var id = element.GetAttribute("x:Name") ?? element.GetAttribute("Name");
        if (!string.IsNullOrEmpty(id))
            elementInfo += $" (ID: {id})";
        
        // Добавляем информацию о ключевых атрибутах
        var keyAttributes = new[] { "Content", "Text", "Title" };
        foreach (var attr in keyAttributes)
        {
            var value = element.GetAttribute(attr);
            if (!string.IsNullOrEmpty(value))
            {
                elementInfo += $" [{attr}: \"{value}\"]";
                break; // Показываем только первый найденный
            }
        }

        Console.WriteLine($"{indentStr}📄 {elementInfo}");

        // Выводим важные атрибуты
        var importantAttrs = element.Attributes
            .Where(kvp => new[] { "Width", "Height", "Margin", "Grid.Row", "Grid.Column" }
                .Contains(kvp.Key))
            .Take(3)
            .ToList();
            
        foreach (var attr in importantAttrs)
        {
            Console.WriteLine($"{indentStr}   @{attr.Key} = \"{attr.Value}\"");
        }

        // Показываем текстовое содержимое, если есть
        if (!string.IsNullOrEmpty(element.TextContent?.Trim()))
        {
            var text = element.TextContent.Trim();
            if (text.Length > 50)
                text = text.Substring(0, 47) + "...";
            Console.WriteLine($"{indentStr}   📝 \"{text}\"");
        }

        // Рекурсивно обходим дочерние элементы
        foreach (var child in element.Children)
        {
            PrintElementTree(child, indent + 1);
        }
    }

    /// <summary>
    /// Подсчитывает общее количество элементов в дереве.
    /// </summary>
    /// <param name="element">Корневой элемент для подсчета.</param>
    /// <returns>Общее количество элементов.</returns>
    private static int CountElements(XamlElement element)
    {
        return 1 + element.Children.Sum(CountElements);
    }

    #endregion
}