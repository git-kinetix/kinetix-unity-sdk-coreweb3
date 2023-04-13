// // ----------------------------------------------------------------------------
// // <copyright file="ByteConverter.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

namespace Kinetix.Utils
{
    public static class ByteConverter
    {
        public static double ConvertBytesToMegabytes(long _Bytes)
        {
            return (_Bytes / 1024f) / 1024f;
        }
    }
}
