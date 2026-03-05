using QRCoder;

namespace Lihtar.Web.Helpers;

public static class QrCodeHelper
{
    public static byte[] ToPngBytes(string text, int pixelsPerModule = 10)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        var qr = new PngByteQRCode(data);
        return qr.GetGraphic(pixelsPerModule);
    }
}