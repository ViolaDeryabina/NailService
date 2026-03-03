using System;

namespace NailServiceApp.Utilities
{
    /// <summary>
    /// Статический класс для форматирования фамилий, имен и отчеств
    /// Предоставляет методы для преобразования ФИО в различные форматы
    /// </summary>
    public static class NameFormatter
    {
        /// <summary>
        /// Форматирует отдельные компоненты ФИО в короткий формат "Фамилия И.О."
        /// </summary>
        /// <param name="lastName">Фамилия</param>
        /// <param name="firstName">Имя</param>
        /// <param name="middleName">Отчество</param>
        /// <returns>Строка вида "Фамилия И.О." или пустая строка</returns>
        public static string FormatToShortName(string lastName, string firstName, string middleName)
        {
            if (string.IsNullOrEmpty(lastName))
                return string.Empty;

            var result = lastName.Trim();

            if (!string.IsNullOrEmpty(firstName))
            {
                result += " " + GetInitial(firstName);
            }

            if (!string.IsNullOrEmpty(middleName))
            {
                result += "." + GetInitial(middleName);
            }

            // Добавляем точку после инициала имени, если есть имя
            if (!string.IsNullOrEmpty(firstName))
            {
                result += ".";
            }

            return result;
        }

        /// <summary>
        /// Форматирует отдельные компоненты ФИО в полный формат "Фамилия Имя Отчество"
        /// </summary>
        /// <param name="lastName">Фамилия</param>
        /// <param name="firstName">Имя</param>
        /// <param name="middleName">Отчество</param>
        /// <returns>Строка вида "Фамилия Имя Отчество" или пустая строка</returns>
        public static string FormatFullName(string lastName, string firstName, string middleName)
        {
            var parts = new[] { lastName, firstName, middleName };
            return string.Join(" ", parts).Trim();
        }

        /// <summary>
        /// Преобразует полное ФИО (из одной строки) в короткий формат "Фамилия И.О."
        /// </summary>
        /// <param name="fullName">Полное ФИО в формате "Фамилия Имя Отчество"</param>
        /// <returns>Строка вида "Фамилия И.О." или исходная строка при ошибке</returns>
        public static string ConvertToShortName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;

            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return string.Empty;

            var result = parts[0]; // Фамилия

            if (parts.Length > 1)
            {
                result += " " + GetInitial(parts[1]); // Инициал имени
            }

            if (parts.Length > 2)
            {
                result += "." + GetInitial(parts[2]); // Инициал отчества
            }

            // Добавляем точку после инициала имени, если есть имя
            if (parts.Length > 1)
            {
                result += ".";
            }

            return result;
        }

        /// <summary>
        /// Получает первый символ строки в верхнем регистре (инициал)
        /// </summary>
        /// <param name="name">Строка (имя или отчество)</param>
        /// <returns>Первый символ строки в верхнем регистре или пустая строка</returns>
        private static string GetInitial(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            return name[0].ToString().ToUpper();
        }
    }
}