using UnityEngine;
using Kinetix.QRCode;

namespace Kinetix.Internal
{
    public class KinetixQRCodeHelper
    {
        public static KinetixQRCodeHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new KinetixQRCodeHelper();
                }

                return instance;
            }
        }

        private static KinetixQRCodeHelper instance;


        private QRCodeGenerator qrGenerator;

        public KinetixQRCodeHelper()
        {
            qrGenerator = new QRCodeGenerator();
        }


        public Texture2D GetQRCodeForUrl(string url, Color lightColor, Color darkColor)
        {
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.H);
            UnityQRCode qrCode = new UnityQRCode(qrCodeData);

            return qrCode.GetGraphic(5, ColorUtility.ToHtmlStringRGBA(darkColor), ColorUtility.ToHtmlStringRGBA(lightColor));
        }

        public Texture2D GetQRCodeForUrl(string url)
        {
            return GetQRCodeForUrl(url, Color.white, Color.black);
        }
    }
}

