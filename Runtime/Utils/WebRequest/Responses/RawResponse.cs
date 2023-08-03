// // ----------------------------------------------------------------------------
// // <copyright file="RawResponse" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine.Networking;

namespace Kinetix.Utils
{
    public class RawResponse: IResponse
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public long ResponseCode { get; set; }

        public string Content => content;

        protected string content;
        
        public void Parse(UnityWebRequest request)
        {
            content = request.downloadHandler.text;
        }
    }
}
