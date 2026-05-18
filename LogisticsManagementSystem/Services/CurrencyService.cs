namespace LogisticsManagementSystem.Services
{
    public class CurrencyService
    {

        private readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>(
                    "https://api.exchangerate.fun/latest?base=USD");

                if (response?.Rates?.ContainsKey("ZAR") == true)
                {
                    return response.Rates["ZAR"];
                }
            }
            catch { }

            // Fallback rate if API fails
            return 18.20m; 
        }
    }

    public class ExchangeRateResponse
    {
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}

