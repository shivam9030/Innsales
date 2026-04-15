namespace InnSales.Common.DTO.Auth
{
    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string Department { get; set; }
        public string OfficeLocation { get; set; }
    }
}