using CurrencyConverter.Services.Result;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CurrencyConverter.Services.Providers
{
    public class FrankfurterApiClient : IExchangeRateProvider
    {
        private readonly HttpClient client;

        public FrankfurterApiClient(HttpClient client)
        {
            this.client = client;
        }

        private record ExchangeRateResponseDTO(decimal Amount, string Base, DateOnly Date, Dictionary<string, decimal> Rates);
        private record ExchangeRateHistoryResponseDTO(string Base, DateOnly Start_date, DateOnly End_date, OrderedDictionary<DateOnly, Dictionary<string, decimal>> Rates);

        private string _ExchangeRatePathWithSymbol = "/v1/latest?base={0}&symbols={1}";
        private string _ExchangeRatePath = "/v1/latest?base={0}";
        private string _ExchangeRateHistoryPath = "v1/{0}..{1}?base={2}";

        public async Task<ServiceResult<CurrencyConvertResult>> Convert(string from_currency, string to_currency, decimal amount, CancellationToken cancellationToken = default)
        {
            string requestUri = string.Format(_ExchangeRatePathWithSymbol, from_currency, to_currency);
            var resp = await client.GetAsync(requestUri, cancellationToken);

            if (resp.IsSuccessStatusCode)
            {
                var respDTO = JsonSerializer.Deserialize<ExchangeRateResponseDTO>(await resp.Content.ReadAsStringAsync(cancellationToken), JsonSerializerOptions.Web);
                var convertResult = new CurrencyConvertResult(amount * respDTO.Rates.First().Value);
                return new ServiceResult<CurrencyConvertResult>(convertResult, true, string.Empty);
            }
            else if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ServiceResult<CurrencyConvertResult>(null, false, "Currency not found");
            }
            else
            {
                return new ServiceResult<CurrencyConvertResult>(null, false, "Error");
            }
        }

        public async Task<ServiceResult<ExchangeRateResult>> GetExchangeRate(string baseCurrency, CancellationToken cancellationToken = default)
        {
            string requestUri = string.Format(_ExchangeRatePath, baseCurrency);
            var resp = await client.GetAsync(requestUri, cancellationToken);

            if (resp.IsSuccessStatusCode)
            {
                var respDTO = JsonSerializer.Deserialize<ExchangeRateResponseDTO>(await resp.Content.ReadAsStringAsync(cancellationToken), JsonSerializerOptions.Web);
                ExchangeRateResult result = new(respDTO.Rates);
                return new ServiceResult<ExchangeRateResult>(result, true);
            }
            else if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ServiceResult<ExchangeRateResult>(null, false, "Currency not found");
            }
            else
            {
                return new ServiceResult<ExchangeRateResult>(null, false, "Error");
            }

        }

        public async Task<ServiceResult<HistoricalExchangeRateResult>> GetExchangeRateHistory(string baseCurrency, DateOnly from, DateOnly to, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            string requestUri = string.Format(_ExchangeRateHistoryPath, from.ToString("o", CultureInfo.InvariantCulture), to.ToString("o", CultureInfo.InvariantCulture), baseCurrency);
            var resp = await client.GetAsync(requestUri, cancellationToken);

            if (resp.IsSuccessStatusCode)
            {
                var respDTO = JsonSerializer.Deserialize<ExchangeRateHistoryResponseDTO>(await resp.Content.ReadAsStringAsync(cancellationToken), JsonSerializerOptions.Web);
                //pagination
                var pages = new OrderedDictionary<DateOnly, Dictionary<string, decimal>>(respDTO.Rates.Skip(pageSize * pageIndex).Take(pageSize));
                return new ServiceResult<HistoricalExchangeRateResult>(new HistoricalExchangeRateResult(pages), true, string.Empty);
            }
            else if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ServiceResult<HistoricalExchangeRateResult>(null, false, "Currency not found");
            }
            else
            {
                return new ServiceResult<HistoricalExchangeRateResult>(null, false, "Error");
            }
        }
    }
}
