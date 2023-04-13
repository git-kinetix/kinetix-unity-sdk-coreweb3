// // ----------------------------------------------------------------------------
// // <copyright file="IProviderWrapper.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix;

// FILE_WEB3

public interface IProviderWrapper
{
    public Task<AnimationMetadata[]> GetAnimationsMetadataOfOwner(string _WalletAddress, List<string> _Contracts);
    public Task<AnimationMetadata> GetAnimationMetadataOfNft(AnimationIds _AnimationIds);
}

