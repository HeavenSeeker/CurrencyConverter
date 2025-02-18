using CurrencyConverter.Services.Providers;
using CurrencyConverter.Services.Result;

namespace CurrencyConverter.Services
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly IExchangeRateProvider exchangeRateProvider;
        private HashSet<string> excludedCurrencies = new() { "TRY", "PLN", "THB", "MXN" };// can be configured in appsettings.json also.

        public CurrencyConverterService(IExchangeRateProvider exchangeRateProvider)
        {
            this.exchangeRateProvider = exchangeRateProvider;
        }

        public async Task<ServiceResult<ExchangeRateResult>> GetExchangeRate(string baseCurrency, CancellationToken cancellationToken)
        {
            return await exchangeRateProvider.GetExchangeRate(baseCurrency, cancellationToken);
        }

        public async Task<ServiceResult<CurrencyConvertResult>> Convert(string from_currency, string to_currency, decimal amount, CancellationToken cancellationToken)
        {
            if (excludedCurrencies.Contains(from_currency) || excludedCurrencies.Contains(to_currency))
            {
                return new ServiceResult<CurrencyConvertResult>(default, false, "TRY, PLN, THB, and MXN currencies are not allowed.");
            }
            return await exchangeRateProvider.Convert(from_currency, to_currency, amount, cancellationToken);
        }

        public async Task<ServiceResult<HistoricalExchangeRateResult>> GetExchangeRateHistory(string baseCurrency, DateOnly from, DateOnly to, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            if (from > to)
            {
                return new ServiceResult<HistoricalExchangeRateResult>(null, false, "Incorrect date range.");
            }
            return await exchangeRateProvider.GetExchangeRateHistory(baseCurrency, from, to, pageIndex, pageSize, cancellationToken);
        }

    }
}
