using CurrencyConverter.Services.Result;

namespace CurrencyConverter.Services
{
    public interface ICurrencyConverterService
    {
        Task<ServiceResult<ExchangeRateResult>> GetExchangeRate(string baseCurrency, CancellationToken cancellationToken);

        Task<ServiceResult<CurrencyConvertResult>> Convert(string from_currency, string to_currency, decimal amount, CancellationToken cancellationToken);

        Task<ServiceResult<HistoricalExchangeRateResult>> GetExchangeRateHistory(string baseCurrency, DateOnly from, DateOnly to, int pageIndex, int pageSize, CancellationToken cancellationToken);

    }
}