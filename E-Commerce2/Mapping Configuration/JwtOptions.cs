namespace E_Commerce2.Mapping_Configuration
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audienc { get; set; } = string.Empty;
        public int Lifetime { get; set; }
        public string SigningKey { get; set; } = string.Empty;
    }
}
