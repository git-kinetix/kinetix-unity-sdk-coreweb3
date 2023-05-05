using System.Collections;
using System.Collections.Generic;
using Kinetix.QRCode;
using UnityEngine;
using UnityEngine.UI;

public class QRCode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        QRCodeGenerator qrGenerator       = new QRCodeGenerator();
        QRCodeData      qrCodeData        = qrGenerator.CreateQrCode("https://www.kinetix.tech", QRCodeGenerator.ECCLevel.H);
        UnityQRCode     qrCode            = new UnityQRCode(qrCodeData);
        Texture2D       qrCodeAsTexture2D = qrCode.GetGraphic(5);
        GetComponent<RawImage>().texture = qrCodeAsTexture2D;
    }

  
}
