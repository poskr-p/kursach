using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace material_design
{
    public class DemandForecastService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:8000";

        public DemandForecastService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> SendForecastRequestAsync(DateTime date, List<int> menuItemIds)
        {
            var request = new
            {
                date = date.ToString("yyyy-MM-dd"),
                menu_item_ids = menuItemIds
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/predict_demand", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TaskResponse>(responseJson);
                return result?.task_id;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке запроса на прогноз: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<ForecastResult> GetForecastResultAsync(string taskId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/status/{taskId}");
                var responseJson = await response.Content.ReadAsStringAsync();

                // ОТЛАДКА: выводим ответ сервера
                System.Diagnostics.Debug.WriteLine($"Status response: {responseJson}");
                MessageBox.Show($"Ответ сервера: {responseJson}", "Отладка", MessageBoxButton.OK, MessageBoxImage.Information);

                response.EnsureSuccessStatusCode();

                var statusResponse = JsonSerializer.Deserialize<TaskStatusResponse>(responseJson);

                if (statusResponse?.status == "SUCCESS" && statusResponse.result != null)
                {
                    var resultJson = statusResponse.result.ToString();
                    return JsonSerializer.Deserialize<ForecastResult>(resultJson);
                }
                else if (statusResponse?.status == "FAILURE")
                {
                    MessageBox.Show($"Ошибка выполнения задачи: {statusResponse.error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Исключение: {ex.Message}\nStackTrace: {ex.StackTrace}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<ForecastResult> GetForecastWithPollingAsync(DateTime date, List<int> menuItemIds, int maxAttempts = 30, int delayMs = 1000)
        {
            var taskId = await SendForecastRequestAsync(date, menuItemIds);
            if (string.IsNullOrEmpty(taskId))
                return null;

            for (int i = 0; i < maxAttempts; i++)
            {
                await Task.Delay(delayMs);
                var result = await GetForecastResultAsync(taskId);
                if (result != null)
                    return result;
            }

            MessageBox.Show("Время ожидания результата истекло. Попробуйте позже.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
    }

    public class TaskResponse
    {
        public string task_id { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }

    public class TaskStatusResponse
    {
        public string task_id { get; set; }
        public string status { get; set; }
        public object result { get; set; }
        public string error { get; set; }
    }

    public class ForecastResult
    {
        public string date { get; set; }
        public List<PredictionItem> predictions { get; set; }
    }

    public class PredictionItem
    {
        public int menu_item_id { get; set; }
        public string item_name { get; set; }
        public double predicted_quantity { get; set; }
    }
}