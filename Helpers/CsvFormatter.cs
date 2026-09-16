namespace DiscordSecurityBot.Helpers
{
    public static class CsvFormatter
    {
        /// <summary>
        /// Преобразует строковое поле в CSV-формат:
        /// всегда заключает в двойные кавычки и экранирует внутренние кавычки удвоением.
        /// </summary>
        public static string FormatCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\"";

            string escaped = field.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }
    }
}