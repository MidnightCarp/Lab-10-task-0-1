namespace Compiler;  // Объявляет пространство имён Compiler, где будут находиться все классы и структуры.

// Структура для хранения позиции текста в файле (номер строки и номер символа)
struct TextPosition
{
    public uint lineNumber;  // Номер строки (начинается с 1)
    public byte charNumber;  // Номер символа в строке (начинается с 0)

    // Конструктор структуры с параметрами (значения по умолчанию: строка 0, символ 0)
    public TextPosition(uint ln = 0, byte c = 0)
    {
        lineNumber = ln;  // Инициализирует номер строки.
        charNumber = c;   // Инициализирует номер символа.
    }
}

// Структура для хранения информации об ошибке
struct Err
{
    public TextPosition errorPosition;  // Позиция ошибки в тексте
    public byte errorCode;              // Код ошибки (соответствует словарю сообщений).

    // Конструктор структуры с параметрами
    public Err(TextPosition errorPosition, byte errorCode)
    {
        this.errorPosition = errorPosition;  // Сохраняет позицию ошибки.
        this.errorCode = errorCode;          // Сохраняет код ошибки.
    }
}