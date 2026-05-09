using System.Diagnostics;

namespace HelloCSharp;

public static class Telemetry
{
    public const string ServiceName = "hello-csharp";

    public static readonly ActivitySource ActivitySource = new(ServiceName);
}
