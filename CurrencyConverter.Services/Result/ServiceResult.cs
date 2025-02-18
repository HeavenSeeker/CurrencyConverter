namespace CurrencyConverter.Services.Result
{
    public record ServiceResult<T>(T Data, bool Succeeded, string Details="");
}