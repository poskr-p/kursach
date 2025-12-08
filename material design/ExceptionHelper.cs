using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Windows;

namespace material_design
{
    public static class ExceptionHelper
    {
        public static string GetFullExceptionMessage(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Ошибка: {ex.Message}");

            // Для исключений Entity Framework Validation
            if (ex is DbEntityValidationException dbEx)
            {
                sb.AppendLine("Ошибки валидации:");
                foreach (var validationError in dbEx.EntityValidationErrors)
                {
                    foreach (var error in validationError.ValidationErrors)
                    {
                        sb.AppendLine($"  • {error.PropertyName}: {error.ErrorMessage}");
                    }
                }
            }

            // Для SqlException (ошибки БД)
            var sqlEx = ex.InnerException as System.Data.SqlClient.SqlException;
            if (sqlEx != null)
            {
                sb.AppendLine($"Код ошибки SQL: {sqlEx.Number}");
                sb.AppendLine($"Сообщение SQL: {sqlEx.Message}");

                // Расшифровка кодов ошибок SQL Server
                switch (sqlEx.Number)
                {
                    case 547: // Foreign key constraint error
                        sb.AppendLine("Ошибка ограничения внешнего ключа. Проверьте существование связанных записей.");
                        break;
                    case 2627: // Unique constraint error
                        sb.AppendLine("Ошибка уникальности. Такая запись уже существует.");
                        break;
                    case 2601: // Duplicate key row error
                        sb.AppendLine("Дублирование ключа.");
                        break;
                    case 515: // NULL insert error
                        sb.AppendLine("Попытка вставить NULL в обязательное поле.");
                        break;
                }
            }

            // Рекурсивно получаем все внутренние исключения
            Exception inner = ex.InnerException;
            int level = 1;
            while (inner != null)
            {
                sb.AppendLine($"\nВнутренняя ошибка (уровень {level}): {inner.Message}");
                inner = inner.InnerException;
                level++;
            }

            return sb.ToString();
        }

        public static void ShowErrorMessage(Exception ex, string context = "сохранения")
        {
            string message = GetFullExceptionMessage(ex);
            MessageBox.Show(message, $"Ошибка {context}", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}