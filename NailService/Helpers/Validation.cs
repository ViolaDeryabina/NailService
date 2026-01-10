using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Класс для валидации ввода русских букв
    /// </summary>
    public static class RussianLettersValidator
    {
        private static readonly Regex _russianLettersRegex = new Regex(@"^[а-яА-ЯёЁ\s\-]*$");
        private static readonly Regex _russianLettersAndNumbersRegex = new Regex(@"^[а-яА-ЯёЁ0-9\s\-]*$");

        /// <summary>
        /// Проверяет, содержит ли строка только русские буквы, пробелы и дефисы
        /// </summary>
        /// <param name="text">Текст для проверки</param>
        /// <returns>true если текст содержит только разрешенные символы</returns>
        public static bool IsValidRussianText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            return _russianLettersRegex.IsMatch(text);
        }

        /// <summary>
        /// Проверяет, содержит ли строка только русские буквы, цифры, пробелы и дефисы
        /// </summary>
        /// <param name="text">Текст для проверки</param>
        /// <returns>true если текст содержит только разрешенные символы</returns>
        public static bool IsValidRussianTextWithNumbers(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            return _russianLettersAndNumbersRegex.IsMatch(text);
        }

        /// <summary>
        /// Устанавливает TextBox для ввода только русских букв
        /// </summary>
        /// <param name="textBox">TextBox для настройки</param>
        /// <param name="allowNumbers">Разрешить цифры (по умолчанию false)</param>
        /// <param name="allowSpaces">Разрешить пробелы (по умолчанию true)</param>
        /// <param name="allowHyphen">Разрешить дефис (по умолчанию true)</param>
        public static void SetupRussianTextBox(TextBox textBox, bool allowNumbers = false, bool allowSpaces = true, bool allowHyphen = true)
        {
            textBox.KeyPress += (sender, e) =>
            {
                var tb = sender as TextBox;

                // Разрешаем управляющие символы (Backspace, Delete, стрелки и т.д.)
                if (char.IsControl(e.KeyChar))
                {
                    return;
                }

                // Проверяем разрешенные символы на основе параметров
                if (IsRussianLetter(e.KeyChar) ||
                    (allowNumbers && char.IsDigit(e.KeyChar)) ||
                    (allowSpaces && e.KeyChar == ' ') ||
                    (allowHyphen && e.KeyChar == '-'))
                {
                    return;
                }

                // Если символ не разрешен, отменяем ввод
                e.Handled = true;
            };
        }

        /// <summary>
        /// Проверяет, является ли символ русской буквой
        /// </summary>
        /// <param name="c">Проверяемый символ</param>
        /// <returns>true если символ - русская буква</returns>
        public static bool IsRussianLetter(char c)
        {
            // Русские буквы в нижнем регистре
            if (c >= 'а' && c <= 'я')
                return true;

            // Русские буквы в верхнем регистре
            if (c >= 'А' && c <= 'Я')
                return true;

            // Буква 'ё' в обоих регистрах
            if (c == 'ё' || c == 'Ё')
                return true;

            return false;
        }

        /// <summary>
        /// Отфильтровывает из текста все не-русские символы
        /// </summary>
        /// <param name="text">Исходный текст</param>
        /// <param name="allowNumbers">Разрешить цифры</param>
        /// <param name="allowSpaces">Разрешить пробелы</param>
        /// <param name="allowHyphen">Разрешить дефис</param>
        /// <returns>Текст, содержащий только русские символы</returns>
        public static string FilterToRussianLetters(string text, bool allowNumbers = false, bool allowSpaces = true, bool allowHyphen = true)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var result = new System.Text.StringBuilder();

            foreach (char c in text)
            {
                if (IsRussianLetter(c) ||
                    (allowNumbers && char.IsDigit(c)) ||
                    (allowSpaces && c == ' ') ||
                    (allowHyphen && c == '-'))
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Проверяет текст и показывает сообщение об ошибке если необходимо
        /// </summary>
        /// <param name="textBox">Проверяемый TextBox</param>
        /// <param name="fieldName">Название поля для сообщения об ошибке</param>
        /// <param name="allowNumbers">Разрешить цифры</param>
        /// <returns>true если текст валиден</returns>
        public static bool ValidateRussianTextBox(TextBox textBox, string fieldName, bool allowNumbers = false)
        {
            if (!IsValidRussianTextWithNumbers(textBox.Text))
            {
                MessageBox.Show($"Поле '{fieldName}' может содержать только русские буквы" +
                                (allowNumbers ? ", цифры" : "") +
                                ", пробелы и дефисы.",
                                "Ошибка ввода",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                textBox.Focus();
                return false;
            }

            return true;
        }
    }
}
