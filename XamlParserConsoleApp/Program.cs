// See https://aka.ms/new-console-template for more information

using xaml_parser;
using xaml_parser.Structures;

using var fileParser = new XamlFileParser();
var document = fileParser.ParseFile(@"C:\Users\UnderKo\RiderProjects\AvaloniaApplication1\AvaloniaApplication1\Views\MainView.axaml");
var button = document?.FindElementById("SaveButton");
var buttodn = document?.FindElementById("asdasd");
Console.ReadKey();