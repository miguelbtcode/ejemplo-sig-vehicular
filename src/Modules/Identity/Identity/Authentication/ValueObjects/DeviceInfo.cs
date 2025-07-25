namespace Identity.Authentication.ValueObjects;

public record DeviceInfo(
    string DeviceId,
    string DeviceName,
    DeviceType Type,
    string Platform,
    string? AppVersion = null
)
{
    public bool IsMobile => Type == DeviceType.Mobile;
    public bool IsWeb => Type == DeviceType.Web;

    public static DeviceInfo CreateMobile(
        string deviceId,
        string deviceName,
        string platform,
        string appVersion
    ) => new(deviceId, deviceName, DeviceType.Mobile, platform, appVersion);

    public static DeviceInfo CreateWeb(string deviceId, string deviceName, string userAgent) =>
        new(deviceId, deviceName, DeviceType.Web, "web");
}

public enum DeviceType
{
    Web,
    Mobile,
}
