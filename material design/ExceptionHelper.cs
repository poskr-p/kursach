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

            var sqlEx = ex.InnerException as System.Data.SqlClient.SqlException;
            if (sqlEx != null)
            {
                sb.AppendLine($"Код ошибки SQL: {sqlEx.Number}");
                sb.AppendLine($"Сообщение SQL: {sqlEx.Message}");

                
                switch (sqlEx.Number)
                {
                    case 547:
                        sb.AppendLine("Ошибка ограничения внешнего ключа. Проверьте существование связанных записей.");
                        break;
                    case 2627: 
                        sb.AppendLine("Ошибка уникальности. Такая запись уже существует.");
                        break;
                    case 2601: 
                        sb.AppendLine("Дублирование ключа.");
                        break;
                    case 515:
                        sb.AppendLine("Попытка вставить NULL в обязательное поле.");
                        break;
                }
            }

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