using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DeployButton
{
    public class TeamCityClient
    {
        private readonly HttpClient _httpClient;
        private TeamCityConfig _config;

        public TeamCityClient(TeamCityConfig config)
        {
            var handler = new HttpClientHandler()
            {
                UseCookies = false,
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            UpdateConfig(config);
        }

        public void UpdateConfig(TeamCityConfig config)
        {
            _config = config;
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.Username}:{_config.Password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        /// <summary>
        /// Проверяет, есть ли уже сборка в очереди или выполняется для данной конфигурации.
        /// </summary>
        public async Task<bool> IsBuildQueuedOrRunningAsync()
        {
            var baseUrl = _config.BaseUrl;
            var buildTypeId = _config.BuildConfigurationId;

            // Проверяем "очередь"
            if (await IsBuildStateActiveAsync(baseUrl, buildTypeId, "queued"))
                return true;

            // Проверяем "выполняется"
            if (await IsBuildStateActiveAsync(baseUrl, buildTypeId, "running"))
                return true;

            return false;
        }

        private async Task<bool> IsBuildStateActiveAsync(string baseUrl, string buildTypeId, string state)
        {
            // Запрашиваем максимум 1 запись — нам важен только факт наличия
            var url = $"{baseUrl}/httpAuth/app/rest/builds?locator=buildType:{buildTypeId},state:{state},count:1";
    
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return false;
                    // Игнорируем ошибки — считаем, что сборок нет
                    return false;
                }

                var xml = await response.Content.ReadAsStringAsync();
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var countAttr = doc.DocumentElement?.Attributes["count"];
                return countAttr != null && int.TryParse(countAttr.Value, out var count) && count > 0;
            }
            catch
            {
                return false; // при ошибке — разрешаем запуск
            }
        }
        /// <summary>
        /// Запускает новую сборку через Remote Trigger.
        /// </summary>
        public async Task TriggerBuildAsync()
        {
            var triggerUrl = $"{_config.BaseUrl}/httpAuth/action.html?add2Queue={_config.BuildConfigurationId}";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, triggerUrl);
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось запустить сборку: {ex.Message}", ex);
            }
        }
    }
}