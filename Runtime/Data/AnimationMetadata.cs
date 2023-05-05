// // ----------------------------------------------------------------------------
// // <copyright file="AnimationMetadata.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Kinetix.Internal;
using UnityEngine;

namespace Kinetix
{
    [Serializable]
    public class AnimationMetadata
    {
		[SerializeField]
        public EOwnership Ownership;
        
		[SerializeField]
        public AnimationIds Ids;

		public string Name;
		public string Description;

        #region Collection

        public string CollectionName;
        public string CollectionDescription;
	    
        #endregion
        
		#region URL

		public string AnimationURL;
        public string IconeURL;
		public DateTime CreatedAt;

		#endregion

		#region Attributes
        
		[SerializeField] public ERarity          EmoteRarity;
        [SerializeField] public NumberAttribute  Exploration;
        [SerializeField] public ElementAttribute VisualElement;
		[SerializeField] public NumberAttribute  Ranking;
		[SerializeField] public NumberAttribute  ArtisticLevel;
		[SerializeField] public NumberAttribute  TechnicalLevel;
		[SerializeField] public BoolAttribute    VisualEffect;
		[SerializeField] public BoolAttribute    ContactFeet;
		[SerializeField] public NumberAttribute  Speed;
		[SerializeField] public NumberAttribute  Aerial;
		[SerializeField] public NumberAttribute  Spin;
		[SerializeField] public NumberAttribute  Stamina;
		[SerializeField] public NumberAttribute  Amplitude;
		[SerializeField] public NumberAttribute  Strength;

		#endregion
    }

	[Serializable]
	public struct NumberAttribute
	{
		public int Value;
		public int Max_Value;
	}

	[Serializable]
	public struct BoolAttribute
	{
		public bool Value;
	}

	[Serializable]
	public struct ElementAttribute
	{
		public EVisualElement Value;
	}
}
