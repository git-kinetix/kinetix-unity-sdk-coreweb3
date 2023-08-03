// // ----------------------------------------------------------------------------
// // <copyright file="ISamplerAuthority.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

namespace Kinetix.Internal
{
    /// <summary>
    /// An effect that has authority on the <see cref="SimulationSampler"/>
    /// </summary>
    public interface ISamplerAuthority
	{
		/// <summary>
		/// Authority on the samplers.<br/>
		/// You can call some methods like "StartNextClip" or "GetAvatarPos".
		/// </summary>                                
		SamplerAuthorityBridge Authority { get; set; }
	}
}
