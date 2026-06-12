using System; // Базовые типы, консоль и т.д.
using System.Collections.Generic; // Для использования Dictionary (ключ — значение)
using System.Text; // Для работы со строками
using Compiler; // Для доступа к структурам TextPosition и Err

namespace Compiler.IO; // Пространство имён для ввода-вывода

// Класс, отвечающий за чтение исходного файла, вывод строк кода, сбор и отображение ошибок
internal class InputOutput
{
    const byte ERRMAX = 9;  // Максимальное количество ошибок для отображения на строке (9)
    public static char Ch { get; private set; }  // Текущий читаемый символ
    public static TextPosition positionNow = new TextPosition();  // Текущая позиция (строка, символ)
    private static string? currentLine;  // Текущая строка файла
    private static string? previousLine;  // Предыдущая строка файла
    public static string? lineToPrint;   // Строка, которую нужно вывести в квадратных скобках
    private static int currentIndex;          // Индекс в текущей строке
    public static List<Err> err = new List<Err>();  // Ошибки текущей строки
    private static List<Err> allErrors = new List<Err>(); // Все ошибки файла
    private static StreamReader? file;  // Поток чтения файла
    private static uint errCount = 0;  // Общий счетчик всех ошибок

    public static string? PreviousLine => previousLine;  // Свойство для доступа к предыдущей строке

    // Словарь с описаниями кодов ошибок
    private static readonly Dictionary<byte, string> errorMessages = new Dictionary<byte, string>
    {
        [0] = "Недопустимый символ (не распознан ни как буква, цифра, оператор, разделитель или начало комментария/константы)",
        [8] = "Символьная константа не закрыта кавычкой",
        [18] = "Комментарий закрыт неправильно или не закрыт",
        [76] = "Целая константа превышает допустимый диапазон (maxint = 32767)"
    };

    // Открытие файла для чтения
    public static void OpenFile(string path)
    {
        file = new StreamReader(path);  // Создаем поток для чтения файла
        err = new List<Err>();  // Очищаем список ошибок текущей строки
        allErrors = new List<Err>();  // Очищаем список всех ошибок
        positionNow.lineNumber = 0;  // Сбрасываем номер строки
        currentIndex = 0;  // Сбрасываем позицию в строке
        ReadNextLine();   // Загружаем первую строку файла
        NextCh();         // Читаем первый символ
    }

    // Чтение следующей строки файла
    private static void ReadNextLine()
    {
        // Если файл не открыт или достигнут конец файла
        if (file == null || file.EndOfStream)
        {
            currentLine = null;  // Сбрасываем текущую строку
            return;  // Выходим из метода
        }
        previousLine = currentLine;  // Сохраняем предыдущую строку
        currentLine = file.ReadLine();  // Читаем следующую строку из файла
        positionNow.lineNumber++;  // Увеличиваем счётчик строк
        currentIndex = 0;  // Сбрасываем позицию в строке на начало
        err.Clear();  // Очищаем список ошибок для новой строки
    }

    // Чтение следующего символа
    public static void NextCh()
    {
        // Если файл не открыт или достигнут конец файла
        if (currentLine == null)
        {
            Ch = '\0';  // Устанавливаем символ конца файла
            return;  // Выходим из метода
        }

        // Если дошли до конца текущей строки - сохраняем для вывода и загружаем следующую
        if (currentIndex >= currentLine.Length)
        {
            lineToPrint = currentLine;  // Сохраняем строку для вывода
            ReadNextLine();   // Переходим к следующей строке

            // Если есть следующая строка, читаем её первый символ
            if (currentLine != null && currentIndex < currentLine.Length)
            {
                Ch = currentLine[currentIndex];  // Берём первый символ новой строки
                positionNow.charNumber = (byte)currentIndex;  // Устанавливаем номер позиции символа
                currentIndex++;  // Увеличиваем индекс для следующего символа
            }
            else
            {
                Ch = '\0';  // Достигнут конец файла
            }
            return;  // Выходим из метода
        }

        // Нормальное чтение символа
        Ch = currentLine[currentIndex];  // Читаем символ из текущей строки
        positionNow.charNumber = (byte)currentIndex;  // Устанавливаем номер позиции символа
        currentIndex++;  // Увеличиваем индекс для следующего символа
    }

    // Вывод текущей строки в квадратных скобках
    public static void PrintCurrentLine()
    {
        if (lineToPrint != null)  // Если есть строка для вывода
        {
            Console.WriteLine($"[ {lineToPrint} ]");  // Выводим строку в квадратных скобках
            lineToPrint = null;  // Сбрасываем строку после вывода
        }
    }

    // Проверка, достигнут ли конец строки
    public static bool IsEndOfLine => currentLine == null || currentIndex >= currentLine.Length;
    
    // Свойства для доступа к данным из LexicalAnalyzer
    public static string? LineToPrint => lineToPrint;  // Строка для вывода
    public static string? CurrentLine => currentLine;  // Текущая строка
    public static int CurrentIndex => currentIndex;  // Текущий индекс
    public static int CurrentLineLength => currentLine?.Length ?? 0;  // Длина текущей строки

    // Чтение следующей строки во время обработки комментария
    public static void ReadNextLineForComment()
    {
        // Если файл не открыт или достигнут конец файла
        if (file == null || file.EndOfStream)
        {
            currentLine = null;  // Сбрасываем текущую строку
            return;  // Выходим из метода
        }
        currentLine = file.ReadLine();  // Читаем следующую строку из файла
        positionNow.lineNumber++;  // Увеличиваем номер строки
        currentIndex = 0;  // Сбрасываем позицию в строке на начало
        // Не очищаем err для комментариев (чтобы ошибки сохранялись)
    }

    // Вывод ошибок текущей строки
    private static void ListErrors()
    {
        int pos = 6 - $"{positionNow.lineNumber} ".Length;  // Выравнивание для номера строки
        string s;  // Переменная для строки вывода
        foreach (Err item in err)  // Проходим по всем ошибкам текущей строки
        {
            s = "**";  // Начало форматирования номера ошибки
            if (errCount < 10) s += "0";  // Добавляем ноль для номеров меньше 10
            s += $"{errCount}**";  // Добавляем номер ошибки
            while (s.Length - 1 < pos + item.errorPosition.charNumber) s += " ";  // Добавляем пробелы для позиционирования
            string errorMsg = GetErrorMessage(item.errorCode);  // Получаем описание ошибки
            s += $"^ ошибка {item.errorCode}: {errorMsg}";  // Добавляем код и описание ошибки
            Console.WriteLine(s);  // Выводим строку с ошибкой
        }
    }

    // Публичный метод для вывода ошибок (из LexicalAnalyzer после обработки строки)
    public static void ListErrorsPublic()
    {
        ListErrors();  // Вызываем приватный метод ListErrors
    }

    // Получение описания ошибки по коду
    private static string GetErrorMessage(byte code)
    {
        return errorMessages.ContainsKey(code) ? errorMessages[code] : "Неизвестная ошибка";  // Возвращаем описание или сообщение об неизвестной ошибке
    }

    // Закрытие файла
    public static void CloseFile()
    {
        file?.Close();
    }

    // Вывод итогового отчета об ошибках
    public static void PrintErrorSummary()
    {
        Console.WriteLine();  // Выводим пустую строку
        if (errCount == 0)  // Если ошибок нет
        {
            Console.WriteLine("Ошибок нет");  // Выводим сообщение об отсутствии ошибок
        }
        else  // Если есть ошибки
        {
            Console.WriteLine($"Всего ошибок: {errCount}");  // Выводим общее количество ошибок
            Console.WriteLine();  // Выводим пустую строку
            Console.WriteLine("Список ошибок:");  // Выводим заголовок списка
            uint num = 0;  // Инициализируем номер ошибки
            foreach (var item in allErrors)  // Проходим по всем ошибкам файла
            {
                num++;  // Увеличиваем номер ошибки
                string errorMsg = GetErrorMessage(item.errorCode);  // Получаем описание ошибки
                Console.WriteLine($"  {num}. Код {item.errorCode}: {errorMsg} (строка {item.errorPosition.lineNumber}, позиция {item.errorPosition.charNumber})");
                // Выводим номер, код, описание и позицию ошибки
            }
        }
    }

    // Добавление ошибки
    public static void Error(byte errorCode, TextPosition position)
    {
        var newErr = new Err(position, errorCode);  // Создаем новую ошибку с позицией и кодом
        if (err.Count <= ERRMAX)  // Если количество ошибок на строке не превышает максимум
        {
            err.Add(newErr);  // Добавляем ошибку в список текущей строки
        }
        allErrors.Add(newErr);  // Добавляем ошибку в список всех ошибок
        ++errCount;  // Увеличиваем общий счетчик ошибок
    }

    // Заглядывание на один символ вперёд (без изменения текущей позиции)
    public static char PeekChar()
    {
        // Если текущей строки нет - возвращаем конец файла
        if (currentLine == null) return '\0';
        // Если есть символы в строке - возвращаем следующий символ
        if (currentIndex < currentLine.Length) return currentLine[currentIndex];
        // Конец строки — смотрим следующую строку
        if (file != null && !file.EndOfStream)
        {
            // Временно читаем следующую строку для проверки
            return '\n'; // Символ перевода строки между строками
        }
        return '\0'; // Конец файла
    }
}
