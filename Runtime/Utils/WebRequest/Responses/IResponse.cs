// // ----------------------------------------------------------------------------
// // <copyright file="IResponse.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine.Networking;

namespace Kinetix.Utils
{
    public interface IResponse
    {
        bool IsSuccess { get; set; }
        string Error { get; set; }
        long ResponseCode { get; set; }
        void Parse(UnityWebRequest request);
    }
}
