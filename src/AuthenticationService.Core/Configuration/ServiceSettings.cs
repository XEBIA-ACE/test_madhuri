namespace AuthenticationService.Core.Configuration;

/// <summary>
/// Service identification settings.
/// </summary>
public class ServiceSettings
{
    public const string SectionName = "ServiceSettings";

    public Guid OrgId { get; set; } = Guid.Parse("95bd4e80-e002-4fe5-ab71-fa85aad9fec8");
    public Guid ProjectId { get; set; } = Guid.Parse("6ac6b43c-29aa-4b94-abde-18897563e8e1");
    public string ServiceId { get; set; } = "AUTH-1";
    public string ServiceName { get; set; } = "Authentication Service";
}
