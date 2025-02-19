using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.WebAPI.DTO
{
    public class TokenModel
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;

    }
}
