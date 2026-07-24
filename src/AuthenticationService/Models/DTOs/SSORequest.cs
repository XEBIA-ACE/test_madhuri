```csharp
namespace AuthenticationService.Models.DTOs;

/// <summary>
/// Request model to initiate SSO login process.
/// </summary>
public class SSORequest
{
    /// <summary>
    /// The pre-registered redirect URI where the response is sent upon successful authentication.
    /// </summary>
    public string RedirectUri { get; set; }
}
```

These test files ensure that integration tests are conducted for the SSO functionality, verifying correct handling of authentication requests, responses, and the use of pre-registered redirect URIs.