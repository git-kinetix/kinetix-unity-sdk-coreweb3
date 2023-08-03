// // ----------------------------------------------------------------------------
// // <copyright file="FileResponse.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using Kinetix.Internal;
using UnityEngine.Networking;

namespace Kinetix.Utils
{
    internal class FileResponse: IResponse
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public long ResponseCode { get; set; }
        
        public void Parse(UnityWebRequest request) {}
    }
}
