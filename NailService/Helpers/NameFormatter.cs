using System;

namespace NailServiceApp.Utilities
{
    public static class NameFormatter
    {
        /// <summary>
        /// Форматирует ФИО в формат "Фамилия И.О."
        /// </summary>
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
                result += "." + GetInitial(middleName) + ".";
            }
            else if (!string.IsNullOrEmpty(firstName))
            {
                result += ".";
            }

            return result;
        }

        /// <summary>
        /// Форматирует полное ФИО из отдельных частей
        /// </summary>
        public static string FormatFullName(string lastName, string firstName, string middleName)
        {
            var parts = new[] { lastName, firstName, middleName };
            return string.Join(" ", parts).Trim();
        }

        /// <summary>
        /// Форматирует существующее полное ФИО в короткий формат
        /// </summary>
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
                result += " " + GetInitial(parts[1]); // Имя
            }

            if (parts.Length > 2)
            {
                result += "." + GetInitial(parts[2]) + "."; // Отчество
            }
            else if (parts.Length > 1)
            {
                result += ".";
            }

            return result;
        }

        private static string GetInitial(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            return name[0].ToString().ToUpper();
        }
    }
}