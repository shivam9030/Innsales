namespace InnSales.Common.DTO.Auth
{
    public class TokenResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresInMinutes { get; set; }
        public IList<string> Roles { get; set; }
    }
}
