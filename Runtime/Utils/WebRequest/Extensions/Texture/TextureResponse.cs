// // ----------------------------------------------------------------------------
// // <copyright file="FileResponse.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Networking;

namespace Kinetix.Utils
{
    public class TextureResponse: IResponse
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public long ResponseCode { get; set; }

        public Texture2D Content => content;

        protected Texture2D content;
        
        public void Parse(UnityWebRequest request)
        {
            DownloadHandlerTexture handler = (DownloadHandlerTexture) request.downloadHandler;
            content = handler.texture;
        }
    }
}
