using System; // Базовые типы, консоль и т.д.
using System.Collections.Generic; // Для использования Dictionary (ключ — значение)
using System.Text; // Для работы со строками
using Compiler; // Для доступа к структурам TextPosition и Err
using Compiler.IO; // Для доступа к классу InputOutput

namespace Compiler.Lexica; // Пространство имён для лексического анализатора.

// Класс для работы с ключевыми словами Pascal
internal class Keywords
{
    // Словарь ключевых слов, сгруппированных по длине слова
    // Внешний словарь: ключ – длина слова (byte), значение – внутренний словарь (имя слова -> код токена)
    public Dictionary<byte, Dictionary<string, byte>> Kw { get; private set; }

    // Конструктор класса – инициализирует словарь ключевых слов
    public Keywords()
        {
            Kw = new Dictionary<byte, Dictionary<string, byte>>();  // Создаем словарь

            // Ключевые слова длиной 2 символа
            var tmp2 = new Dictionary<string, byte>
            {
                ["do"] = LexicalAnalyzer.dosy,       // 'do' -> код ключевого слова dosy
                ["if"] = LexicalAnalyzer.ifsy,       // 'if' -> код ключевого слова ifsy
                ["in"] = LexicalAnalyzer.insy,       // 'in' -> код ключевого слова insy
                ["of"] = LexicalAnalyzer.ofsy,       // 'of' -> код ключевого слова ofsy
                ["or"] = LexicalAnalyzer.orsy,       // 'or' -> код ключевого слова orsy
                ["to"] = LexicalAnalyzer.tosy        // 'to' -> код ключевого слова tosy
            };
            Kw[2] = tmp2;  // Добавляем в главный словарь по ключу длины 2

            // Ключевые слова длиной 3 символа
            var tmp3 = new Dictionary<string, byte>
            {
                ["end"] = LexicalAnalyzer.endsy,     // 'end' -> код ключевого слова endsy
                ["var"] = LexicalAnalyzer.varsy,     // 'var' -> код ключевого слова varsy
                ["div"] = LexicalAnalyzer.divsy,     // 'div' -> код ключевого слова divsy
                ["and"] = LexicalAnalyzer.andsy,     // 'and' -> код ключевого слова andsy
                ["not"] = LexicalAnalyzer.notsy,     // 'not' -> код ключевого слова notsy
                ["for"] = LexicalAnalyzer.forsy,     // 'for' -> код ключевого слова forsy
                ["mod"] = LexicalAnalyzer.modsy,     // 'mod' -> код ключевого слова modsy
                ["nil"] = LexicalAnalyzer.nilsy,     // 'nil' -> код ключевого слова nilsy
                ["set"] = LexicalAnalyzer.setsy      // 'set' -> код ключевого слова setsy
            };
            Kw[3] = tmp3;  // Добавляем в главный словарь по ключу длины 3

            // Ключевые слова длиной 4 символа
            var tmp4 = new Dictionary<string, byte>
            {
                ["then"] = LexicalAnalyzer.thensy,   // 'then' -> код ключевого слова thensy
                ["else"] = LexicalAnalyzer.elsesy,   // 'else' -> код ключевого слова elsesy
                ["case"] = LexicalAnalyzer.casesy,   // 'case' -> код ключевого слова casesy
                ["file"] = LexicalAnalyzer.filesy,   // 'file' -> код ключевого слова filesy
                ["goto"] = LexicalAnalyzer.gotosy,   // 'goto' -> код ключевого слова gotosy
                ["type"] = LexicalAnalyzer.typesy,   // 'type' -> код ключевого слова typesy
                ["with"] = LexicalAnalyzer.withsy    // 'with' -> код ключевого слова withsy
            };
            Kw[4] = tmp4;  // Добавляем в главный словарь по ключу длины 4

            // Ключевые слова длиной 5 символов
            var tmp5 = new Dictionary<string, byte>
            {
                ["begin"] = LexicalAnalyzer.beginsy, // 'begin' -> код ключевого слова beginsy
                ["while"] = LexicalAnalyzer.whilesy, // 'while' -> код ключевого слова whilesy
                ["array"] = LexicalAnalyzer.arraysy, // 'array' -> код ключевого слова arraysy
                ["const"] = LexicalAnalyzer.constsy, // 'const' -> код ключевого слова constsy
                ["label"] = LexicalAnalyzer.labelsy, // 'label' -> код ключевого слова labelsy
                ["until"] = LexicalAnalyzer.untilsy  // 'until' -> код ключевого слова untilsy
            };
            Kw[5] = tmp5;  // Добавляем в главный словарь по ключу длины 5

            // Ключевые слова длиной 6 символов
            var tmp6 = new Dictionary<string, byte>
            {
                ["downto"] = LexicalAnalyzer.downtosy,   // 'downto' -> код ключевого слова downtosy
                ["packed"] = LexicalAnalyzer.packedsy,   // 'packed' -> код ключевого слова packedsy
                ["record"] = LexicalAnalyzer.recordsy,   // 'record' -> код ключевого слова recordsy
                ["repeat"] = LexicalAnalyzer.repeatsy    // 'repeat' -> код ключевого слова repeatsy
            };
            Kw[6] = tmp6;  // Добавляем в главный словарь по ключу длины 6

            // Ключевое слово длиной 7 символов
            var tmp7 = new Dictionary<string, byte> { ["program"] = LexicalAnalyzer.programsy };  // 'program' -> код ключевого слова programsy
            Kw[7] = tmp7;  // Добавляем в главный словарь по ключу длины 7

            // Ключевое слово длиной 8 символов
            var tmp8 = new Dictionary<string, byte> { ["function"] = LexicalAnalyzer.functionsy };  // 'function' -> код ключевого слова functionsy
            Kw[8] = tmp8;  // Добавляем в главный словарь по ключу длины 8

            // Ключевое слово длиной 9 символов
            var tmp9 = new Dictionary<string, byte> { ["procedure"] = LexicalAnalyzer.procedurensy };  // 'procedure' -> код ключевого слова procedurensy
            Kw[9] = tmp9;  // Добавляем в главный словарь по ключу длины 9
        }

    // Проверка, является ли строка ключевым словом
    // Если да – возвращает код токена этого ключевого слова
    // Если нет – возвращает код ident (2) – идентификатор.
    public byte CheckKeyword(string name)
        {
        // Если есть словарь для данной длины и в нём содержится имя – вернуть соответствующий код
        if (Kw.ContainsKey((byte)name.Length) && Kw[(byte)name.Length].ContainsKey(name))
                return Kw[(byte)name.Length][name];
            return LexicalAnalyzer.ident;  // Не ключевое слово – значит идентификатор
    }

    // По коду токена возвращает строковое имя ключевого слова
    public string GetKeywordName(byte code)
        {
        // Перебираем все группы слов (по длине)
        foreach (var lenGroup in Kw)
            {
            // Перебираем пары (имя, код) внутри группы
            foreach (var kv in lenGroup.Value)
                {
                    if (kv.Value == code)  // Если код совпадает
                        return kv.Key;  // Возвращаем имя ключевого слова
                }
            }
            return string.Empty;  // Если не найдено - возвращаем пустую строку
        }
}
