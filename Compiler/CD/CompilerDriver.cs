using System; // Базовые типы, консоль и т.д.
using System.Collections.Generic; // Для использования Dictionary (ключ — значение)
using System.Text; // Для работы со строками
using Compiler; // Для доступа к структурам TextPosition и Err
using Compiler.IO; // Для доступа к классу InputOutput
using Compiler.Lexica; // Для доступа к классу Lexica

namespace Compiler.CD; // Пространство имён для запуска программы

// Главный класс компилятора - точка входа в программу
internal class CompilerDriver
{
        // Точка входа в программу
        static void Main(string[] args)
        {
            string inputFile = "test.pas";  // Имя файла по умолчанию
            if (args.Length > 0)  // Если передан аргумент командной строки
                inputFile = args[0];  // Используем его как имя файла

            InputOutput.OpenFile(inputFile);  // Открываем файл для чтения
            LexicalAnalyzer lex = new LexicalAnalyzer();  // Создаем лексический анализатор
            lex.Run();  // Запускаем лексический анализ
            InputOutput.CloseFile();  // Закрываем файл
            InputOutput.PrintErrorSummary();  // Выводим итоговый отчет об ошибках
        }
}
