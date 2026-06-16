using System.Text;

namespace WebhookGateway.LoadGenerator;

internal static class PayloadGenerator
{
    public static string Generate(int sizeKb)
    {
        var targetBytes = sizeKb * 1024;
        const string prefix = "{\"event\":\"test\",\"payload\":\"";
        const string suffix = "\"}";
        var paddingLength = Math.Max(0, targetBytes - Encoding.UTF8.GetByteCount(prefix) - Encoding.UTF8.GetByteCount(suffix));

        return prefix + new string('x', paddingLength) + suffix;
    }
}
