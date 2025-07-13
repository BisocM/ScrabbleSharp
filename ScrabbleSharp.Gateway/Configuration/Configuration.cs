namespace ScrabbleSharp.Gateway.Configuration;

public class Configuration
{
    public class CorsSettings
    {
        public string[] AllowedOrigins { get; set; } = [];
    }
}