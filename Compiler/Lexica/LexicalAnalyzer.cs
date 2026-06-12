using System; // Базовые типы, консоль и т.д.
using System.Collections.Generic; // Для использования Dictionary (ключ — значение)
using System.Text; // Для работы со строками
using Compiler; // Для доступа к структурам TextPosition и Err
using Compiler.IO; // Для доступа к классу InputOutput

namespace Compiler.Lexica; // Пространство имён для лексического анализатора

// Лексический анализатор - разбивает исходный код на токены (лексемы)
internal class LexicalAnalyzer
{
        // Коды лексем (типов токенов)
        public const byte
            star = 21,           // символ '*'
            slash = 60,          // символ '/'
            equal = 16,          // символ '='
            comma = 20,          // символ ','
            semicolon = 14,      // символ ';'
            colon = 5,           // символ ':'
            point = 61,          // символ '.'
            arrow = 62,          // символ '^'
            leftpar = 9,         // символ '('
            rightpar = 4,        // символ ')'
            lbracket = 11,       // символ '['
            rbracket = 12,       // символ ']'
            flpar = 63,          // левая круглая скобка комментария '('
            frpar = 64,          // правая круглая скобка комментария ')'
            later = 65,          // символ '<'
            greater = 66,        // символ '>'
            laterequal = 67,     // символ '<='
            greaterequal = 68,   // символ '>='
            latergreater = 69,   // символ '<>'
            plus = 70,           // символ '+'
            minus = 71,          // символ '-'
            lcomment = 72,       // начало комментария '('
            rcomment = 73,       // конец комментария ')'
            assign = 51,         // оператор присваивания ':='
            twopoints = 74,      // два двоеточия '..'
            ident = 2,           // идентификатор (переменная, функция и т.д.)
            floatc = 82,         // вещественная константа
            intc = 15,           // целая константа
            casesy = 31,         // ключевое слово 'case'
            elsesy = 32,         // ключевое слово 'else'
            filesy = 57,         // ключевое слово 'file'
            gotosy = 33,         // ключевое слово 'goto'
            thensy = 52,         // ключевое слово 'then'
            typesy = 34,         // ключевое слово 'type'
            untilsy = 53,        // ключевое слово 'until'
            dosy = 54,           // ключевое слово 'do'
            withsy = 37,         // ключевое слово 'with'
            ifsy = 56,           // ключевое слово 'if'
            insy = 100,          // ключевое слово 'in'
            ofsy = 101,          // ключевое слово 'of'
            orsy = 102,          // ключевое слово 'or'
            tosy = 103,          // ключевое слово 'to'
            endsy = 104,         // ключевое слово 'end'
            varsy = 105,         // ключевое слово 'var'
            divsy = 106,         // ключевое слово 'div'
            andsy = 107,         // ключевое слово 'and'
            notsy = 108,         // ключевое слово 'not'
            forsy = 109,         // ключевое слово 'for'
            modsy = 110,         // ключевое слово 'mod'
            nilsy = 111,         // ключевое слово 'nil'
            setsy = 112,         // ключевое слово 'set'
            beginsy = 113,       // ключевое слово 'begin'
            whilesy = 114,       // ключевое слово 'while'
            arraysy = 115,       // ключевое слово 'array'
            constsy = 116,       // ключевое слово 'const'
            labelsy = 117,       // ключевое слово 'label'
            downtosy = 118,      // ключевое слово 'downto'
            packedsy = 119,      // ключевое слово 'packed'
            recordsy = 120,      // ключевое слово 'record'
            repeatsy = 121,      // ключевое слово 'repeat'
            programsy = 122,     // ключевое слово 'program'
            functionsy = 123,    // ключевое слово 'function'
            procedurensy = 124;  // ключевое слово 'procedure'

        byte symbol;  // Код текущего распознанного токена
        TextPosition token;  // Позиция токена в исходном коде (строка, символ)
    string addrName = string.Empty;  // Имя идентификатора (для переменных, функций и т.д.)
    int nmb_int;  // Значение целочисленной константы
        float nmb_float;  // Значение вещественной константы
        char one_symbol;  // Символьная константа (сохраняется, но не выводится отдельно)

    static Keywords keywords = new Keywords();  // Объект для проверки ключевых слов

        // Запуск лексического анализа всего файла
        public void Run()
        {
            while (InputOutput.Ch != '\0')  // Пока не достигнут конец файла
            {
                NextSym();  // Получаем следующий токен
                PrintToken();  // Выводим информацию о токене (код + возможные данные)
                               // Если после чтения токена есть строка, ожидающая вывода в квадратных скобках – выводим
            if (InputOutput.LineToPrint != null)
                {
                    InputOutput.PrintCurrentLine();  // Выводим строку в квадратных скобках
                }
            }
        }

        // Вывод информации о токене
        void PrintToken()
        {
            Console.Write($"{symbol} ");  // Выводим числовой код токена
        if (symbol == ident)  // Если токен - идентификатор
                Console.Write($"({addrName}) ");  // Выводим имя идентификатора
            else if (symbol == intc)  // Если токен - целая константа
                Console.Write($"({nmb_int}) ");  // Выводим значение целой константы
            else if (symbol == floatc)  // Если токен - вещественная константа или символьная
                Console.Write($"({nmb_float}) ");  // Выводим значение
            else if (symbol == LexicalAnalyzer.assign)  // Если токен - оператор присваивания
                Console.Write("(:=) ");  // Выводим обозначение
            else if (symbol == LexicalAnalyzer.twopoints)  // Если токен - два двоеточия
                Console.Write("(..) ");  // Выводим обозначение
            else  // Для остальных токенов
            {
                string keywordName = keywords.GetKeywordName(symbol);  // Получаем имя ключевого слова по коду
                if (!string.IsNullOrEmpty(keywordName))  // Если это ключевое слово
                    Console.Write($"({keywordName}) ");  // Выводим его имя
            }
            Console.WriteLine();  // Переход на новую строку после вывода токена
    }

        // Получение следующего символа
        public byte NextSym()
        {
            // пропуск пробелов и комментариев
            while (true)  // Бесконечный цикл (продолжаем пока не найдем токен)
            {
                while (InputOutput.Ch == ' ' || InputOutput.Ch == '\t') InputOutput.NextCh();  // Пропускаем пробелы и табуляцию

                token.lineNumber = InputOutput.positionNow.lineNumber;  // Сохраняем номер строки токена
                token.charNumber = InputOutput.positionNow.charNumber;  // Сохраняем позицию в строке токена

                // Обработка комментария вида (* ... *)
                if (InputOutput.Ch == '(' && InputOutput.PeekChar() == '*')  // Если начало комментария (*
                {
                    SkipCommentOldStyle();  // Пропускаем комментарий
                    if (InputOutput.Ch == '\0') return 0; // Если конец файла внутри комментария - возвращаем 0
                    continue;  // Продолжаем поиск токена
                }
                // Обработка комментария вида { ... }
                if (InputOutput.Ch == '{')  // Если начало комментария {
                {
                    SkipCommentBrace();  // Пропускаем комментарий
                    if (InputOutput.Ch == '\0') return 0; // Если конец файла внутри комментария - возвращаем 0
                    continue;  // Продолжаем поиск токена
                }
                break;  // Найдена начала токена, выходим из цикла
            }

            // распознавание лексем
            // Если символ - буква (начало идентификатора или ключевого слова)
            if (char.IsLetter(InputOutput.Ch))
            {
                string name = "";  // Создаем строку для имени
                uint startLine = InputOutput.positionNow.lineNumber;  // Запоминаем строку начала идентификатора
                while (char.IsLetterOrDigit(InputOutput.Ch) && InputOutput.positionNow.lineNumber == startLine)  // Пока символы буквы или цифры и та же строка
                {
                    name += InputOutput.Ch;  // Добавляем символ к имени
                    InputOutput.NextCh();  // Читаем следующий символ
                }
                symbol = keywords.CheckKeyword(name);  // Проверяем, является ли имя ключевым словом
                if (symbol == ident)  // Если не ключевое слово
                    addrName = name;  // Сохраняем имя идентификатора
                return symbol;  // Возвращаем код токена
            }

            // Если символ - цифра (начало числа)
            if (char.IsDigit(InputOutput.Ch))
            {
                nmb_int = 0;  // Инициализируем целую часть нулем
                const int maxint = 32767; // Максимальное целое для Pascal (стандартный диапазон)
                while (char.IsDigit(InputOutput.Ch))  // Пока символы - цифры
                {
                    byte digit = (byte)(InputOutput.Ch - '0');  // Преобразуем символ в цифру
                    if (nmb_int <= (maxint - digit) / 10)  // Если число не превышает максимум
                        nmb_int = nmb_int * 10 + digit;  // Добавляем цифру к числу
                    else  // Если число превышает максимум
                    {
                        InputOutput.Error(76, InputOutput.positionNow);  // Записываем ошибку 76 (переполнение)
                        while (char.IsDigit(InputOutput.Ch)) InputOutput.NextCh();  // Пропускаем оставшиеся цифры
                        nmb_int = 0;  // Сбрасываем число
                    }
                    InputOutput.NextCh();  // Читаем следующий символ
                }

                // Проверка на вещественную константу
                if (InputOutput.Ch == '.' && char.IsDigit(InputOutput.PeekChar()))  // Если точка и следующая цифра
                {
                    InputOutput.NextCh();// Пропускаем точку
                    float frac = 0, factor = 0.1f;  // Инициализируем дробную часть
                    while (char.IsDigit(InputOutput.Ch))  // Пока символы - цифры
                    {
                        frac += (InputOutput.Ch - '0') * factor;  // Добавляем цифру к дробной части
                        factor *= 0.1f;  // Уменьшаем множитель
                        InputOutput.NextCh();  // Читаем следующий символ
                    }
                    nmb_float = nmb_int + frac;  // Складываем целую и дробную части
                    symbol = floatc;  // Устанавливаем тип токена как вещественная константа
                }
                else
                    symbol = intc;  // Иначе это целая константа

                return symbol;  // Возвращаем код токена
            }
            // Если символ - кавычка (символьная константа)
            if (InputOutput.Ch == '\'')
            {
                InputOutput.NextCh();  // Пропускаем открывающую кавычку
                if (InputOutput.Ch == '\'')  // Если следующая кавычка (пустая строка '')
                    one_symbol = '\'';  // Устанавливаем символ
                else
                {
                    one_symbol = InputOutput.Ch;  // Запоминаем символ
                    InputOutput.NextCh();  // Читаем следующий символ
                    if (InputOutput.Ch != '\'')  // Если нет закрывающей кавычки
                        InputOutput.Error(8, InputOutput.positionNow);  // Записываем ошибку 8
                }
                InputOutput.NextCh();  // Пропускаем закрывающую кавычку
                symbol = LexicalAnalyzer.floatc;  // Устанавливаем тип токена (условно, символьная константа)
                return symbol;  // Возвращаем код токена
            }

            // операторы и разделители
            switch (InputOutput.Ch)  // Проверяем специальный символ
            {
                case '<':  // Символ '<'
                    InputOutput.NextCh();  // Читаем следующий символ
                    if (InputOutput.Ch == '=')  // Если '='
                    { symbol = laterequal; InputOutput.NextCh(); }  // Это '<='
                    else if (InputOutput.Ch == '>')  // Если '>'
                    { symbol = latergreater; InputOutput.NextCh(); }  // Это '<>'
                    else
                        symbol = later;  // Иначе это '<'
                    break;
                case '>':  // Символ '>'
                    InputOutput.NextCh();  // Читаем следующий символ
                    if (InputOutput.Ch == '=')  // Если '='
                    { symbol = greaterequal; InputOutput.NextCh(); }  // Это '>='
                    else
                        symbol = greater;  // Иначе это '>'
                    break;
                case ':':  // Символ ':'
                    InputOutput.NextCh();  // Читаем следующий символ
                    if (InputOutput.Ch == '=')  // Если '='
                    { symbol = assign; InputOutput.NextCh(); }  // Это ':='
                    else
                        symbol = colon;  // Иначе это ':'
                    break;
                case ';': symbol = semicolon; InputOutput.NextCh(); break;  // Символ ';'
                case '.': symbol = point; InputOutput.NextCh(); break;  // Символ '.'
                case ',': symbol = comma; InputOutput.NextCh(); break;  // Символ ','
                case '=': symbol = equal; InputOutput.NextCh(); break;  // Символ '='
                case '+': symbol = plus; InputOutput.NextCh(); break;  // Символ '+'
                case '-': symbol = minus; InputOutput.NextCh(); break;  // Символ '-'
                case '*': symbol = star; InputOutput.NextCh(); break;  // Символ '*'
                case '/': symbol = slash; InputOutput.NextCh(); break;  // Символ '/'
                case '(': symbol = leftpar; InputOutput.NextCh(); break;  // Символ '('
                case ')': symbol = rightpar; InputOutput.NextCh(); break;  // Символ ')'
                case '[': symbol = lbracket; InputOutput.NextCh(); break;  // Символ '['
                case ']': symbol = rbracket; InputOutput.NextCh(); break;  // Символ ']'
                case '^': symbol = arrow; InputOutput.NextCh(); break;  // Символ '^'
                default:  // Любой другой символ
                    InputOutput.Error(0, InputOutput.positionNow);  // Записываем ошибку 0 (недопустимый символ)
                    InputOutput.NextCh();  // Читаем следующий символ
                    symbol = 0;  // Устанавливаем код токена 0
                    break;
            }
            return symbol;  // Возвращаем код токена
        }

        // Заглядывание на один символ вперёд без изменения позиции
        char PeekChar()
        {
            return InputOutput.PeekChar();  // Вызываем метод InputOutput
        }

        // Пропуск комментария в стиле (* ... *)
        void SkipCommentOldStyle()
        {
            InputOutput.NextCh(); // пропустить '('
            InputOutput.NextCh(); // пропустить '*'
            bool errorReported = false;  // Флаг: выдана ли уже ошибка
            while (InputOutput.Ch != '\0')  // Пока не конец файла
            {
                if (InputOutput.Ch == '*')  // Если встретили звездочку
                {
                    char next = InputOutput.PeekChar();  // Смотрим следующий символ
                    if (next == ')')  // Если это закрывающая скобка
                    {
                        InputOutput.NextCh(); // Пропускаем звездочку
                        InputOutput.NextCh(); // Пропускаем скобку
                        return; // комментарий успешно закрыт  // Выходим
                    }
                    else if (!errorReported)  // Если ошибка еще не выдана
                    {
                        // После * идёт не ), это ошибка 18
                        InputOutput.Error(18, InputOutput.positionNow);  // Записываем ошибку 18
                        errorReported = true;  // Устанавливаем флаг
                    }
                }
                InputOutput.NextCh();  // Читаем следующий символ
            }
            // Если цикл завершился из-за конца файла, ошибка уже была зафиксирована (если встречалась '*')
        }

        // Пропуск комментария в фигурных скобках { ... }
        void SkipCommentBrace()
        {
            InputOutput.NextCh(); // пропустить '{'
            while (InputOutput.Ch != '}' && InputOutput.Ch != '\0')  // Пока не встретили '}' или конец файла
            {
                // Если достигнут конец строки, выводим её и и читаем следующую
                if (InputOutput.CurrentIndex >= InputOutput.CurrentLineLength)
                {
                    InputOutput.lineToPrint = InputOutput.CurrentLine;  // Сохраняем строку для вывода
                    InputOutput.PrintCurrentLine();  // Выводим строку
                    InputOutput.ReadNextLineForComment();  // Читаем следующую строку
                    if (InputOutput.CurrentLine == null) break;  // Если конец файла - выходим
                    continue;  // Продолжаем цикл
                }
                InputOutput.NextCh();  // Читаем следующий символ
            }
            if (InputOutput.Ch == '}')  // Если встретили закрывающую скобку
            {
                InputOutput.NextCh();  // Пропускаем её
            }
            else  // Иначе (конец файла без закрывающей скобки)
            {
                InputOutput.Error(18, InputOutput.positionNow);  // Записываем ошибку 18
            }
        }
}
