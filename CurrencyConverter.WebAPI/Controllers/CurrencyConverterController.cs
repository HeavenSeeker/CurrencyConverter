using CurrencyConverter.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.WebAPI.Controllers
{
    [ApiController]
    [Route("api")]
    [ResponseCache(Duration = 2 * 60 * 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["*"])]
    public class CurrencyConverterController : ControllerBase
    {
        private readonly ICurrencyConverterService currencyConverterService;
        private readonly ILogger<CurrencyConverterController> _logger;

        public CurrencyConverterController(ICurrencyConverterService currencyConverterService,
                                           ILogger<CurrencyConverterController> logger)
        {
            this.currencyConverterService = currencyConverterService;
            _logger = logger;
        }

        [HttpGet("GetExchangeRate")]
        public async Task<IActionResult> GetExchangeRate([Required] string base_currency,
                                                         CancellationToken cancellationToken)
        {
            _logger.LogInformation("-->> GetExchangeRate request");
            var result = await currencyConverterService.GetExchangeRate(base_currency, cancellationToken);
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(new { message = result.Details });
            }
        }

        [HttpGet("Convert")]
        public async Task<IActionResult> Convert([Required] string from_currency,
                                                 [Required] string to_currency,
                                                 [Required] decimal amount,
                                                 CancellationToken cancellationToken)
        {
            _logger.LogInformation("-->> Convert request");
            var result = await currencyConverterService.Convert(from_currency, to_currency, amount, cancellationToken);
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(new { message = result.Details });
            }
        }

        [HttpGet("History")]
        public async Task<IActionResult> GetExchangeRateHistory([Required] string base_currency,
                                                                [Required] DateOnly from,
                                                                [Required] DateOnly to,
                                                                [Required] int pageIndex,
                                                                [Required] int pageSize,
                                                                CancellationToken cancellationToken)
        {
            _logger.LogInformation("-->> GetExchangeRateHistory request");
            var result = await currencyConverterService.GetExchangeRateHistory(base_currency, from, to, pageIndex, pageSize, cancellationToken);
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(new { message = result.Details });
            }
        }
    }
}
