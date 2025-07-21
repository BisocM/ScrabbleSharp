namespace ScrabbleSharp.Gateway.Configuration;

/// <summary>
///     Root configuration class for application settings.
/// </summary>
public class Configuration
{
    /// <summary>
    ///     Defines CORS (Cross-Origin Resource Sharing) settings.
    /// </summary>
    public class CorsSettings
    {
        /// <summary>
        ///     Gets or sets the array of allowed origin URLs for CORS requests.
        /// </summary>
        public string[] AllowedOrigins { get; set; } = [];
    }
}