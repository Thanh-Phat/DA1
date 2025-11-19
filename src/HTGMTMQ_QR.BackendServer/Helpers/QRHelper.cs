using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
namespace HTGMTMQ_QR.BackendServer.Helpers
{
    public static class QRHelper
    {
        public static byte[] GenerateQR(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            using (Bitmap qrImage = qrCode.GetGraphic(20))
            using (MemoryStream ms = new MemoryStream())
            {
                qrImage.Save(ms, ImageFormat.Png);
                return ms.ToArray(); // trả file PNG dạng byte[] để lưu DB hoặc file
            }
        }
    }
}
