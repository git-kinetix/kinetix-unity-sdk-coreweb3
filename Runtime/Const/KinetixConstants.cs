// // ----------------------------------------------------------------------------
// // <copyright file="BlockchainConstants.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Kinetix.Internal
{
    public static class KinetixConstants
    {
        public const string version = "1.0.0";

        public static bool C_ShouldUGCBeAvailable = false;

        public static readonly int c_TimeOutCreateQRCode = 305;
        
        
        #region StreamingAssets
        
        public const string C_FreeAnimationsAssetPluginPath = "Packages/com.kinetix.coreweb3/Resources/FreeAnimations";
        
        public static string C_FreeAnimationsManifestPath => "Kinetix/FreeEmoteManifest.json";
        public const string C_FreeAnimationsAssetSAPath = "Assets/StreamingAssets/Kinetix/FreeAnimations";
        public const string C_FreeAnimationsPath = "Kinetix/FreeAnimations";
        public const string C_FreeCustomAnimationsPath = "Kinetix/CustomFreeAnimations";
        public const string C_FreeCustomAnimationsAssetSAPath = "Assets/StreamingAssets/Kinetix/CustomFreeAnimations";
        
        #endregion

        
        #region KINETIX BACKEND
       
        public const string c_BaseKinetixURL = "https://minting.kinetix.tech";

        public const string c_KinetixCollectionEndpoint = "collections";
        #endregion
        
        #region ALCHEMY
        
        public const string c_BaseAlchemyURL                 = "https://polygon-mainnet.g.alchemy.com/nft/v2";
        public const string c_AlchemyGetNFTsForOwnerEndpoint = "getNFTs";
        public const string c_AlchemyGetNFTMetadataEndpoint  = "getNFTMetadata";

        #endregion
        
        #region FREE ANIMATIONS FROM KINETIX
       
        public const string c_FreeCollectionAddress = "0x935e7f5c9a70b9cc9eea4133e7d4bf01f03b5918";
        public static readonly List<string> c_FreeTokenIds = new List<string>()
        {
            "1",
            "2",
            "3",
            "4",
            "5"
        };
        
        #endregion
        
    }
}
