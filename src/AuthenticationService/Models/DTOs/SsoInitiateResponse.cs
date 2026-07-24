namespace AuthenticationService.Models.DTOs;

/// <summary>
/// Response returned by the SSO initiation endpoint.
/// Contains the identity provider authorization URL that the client
/// should redirect the browser to in order to begin the SSO flow.
/// </summary>
public class SsoInitiateResponse
{
    /// <summary>
    /// The fully-constructed authorization URL for the configured identity
    /// provider. The client must redirect the browser to this URL to start
    /// the SSO authentication flow.
    /// </summary>
    public required string RedirectUrl { get; set; }
}
