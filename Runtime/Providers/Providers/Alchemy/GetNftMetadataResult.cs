// // ----------------------------------------------------------------------------
// // <copyright file="GetNftMetadataResult.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

// FILE_WEB3

using System;
using System.Collections.Generic;
using Kinetix.Internal;

namespace Kinetix.Utils.Alchemy
{
    [Serializable]
    public class GetNftMetadataResult : WebRequestHandler.JsonWebResponse
    {
        public Contract         contract;
        public Id               id;
        public string           title;
        public string           description;
        public TokenUri         tokenUri;
        public List<Medium>     media;
        public Metadata         metadata;
        public DateTime         timeLastUpdated;
        public ContractMetadata contractMetadata;

        public AnimationMetadata Deserialize()
        {
            NftAttribute explorationAttribute = metadata.attributes.Find(trait => trait.trait_type.Equals("Exploration"));
            NftAttribute rankingAttribute     = metadata.attributes.Find(trait => trait.trait_type.Equals("Ranking"));
            NftAttribute artisticLevelAttribute =
                metadata.attributes.Find(trait => trait.trait_type.Equals("Artistic level"));
            NftAttribute technicalLevelAttribute =
                metadata.attributes.Find(trait => trait.trait_type.Equals("Technical level"));
            NftAttribute visualElementAttribute =
                metadata.attributes.Find(trait => trait.trait_type.Equals("Visual Element"));
            NftAttribute visualEffectAttribute =
                metadata.attributes.Find(trait => trait.trait_type.Equals("Visual Effect"));
            NftAttribute contactFeetAttribute =
                metadata.attributes.Find(trait => trait.trait_type.Equals("Contact Feet"));
            NftAttribute aerialAttribute = metadata.attributes.Find(trait => trait.trait_type.Equals("Aerial"));
            NftAttribute amplitudeAttribute =
                metadata.attributes.Find(trait => trait.trait_type.Equals("Amplitude"));
            NftAttribute speedAttribute    = metadata.attributes.Find(trait => trait.trait_type.Equals("Speed"));
            NftAttribute spinAttribute     = metadata.attributes.Find(trait => trait.trait_type.Equals("Spin"));
            NftAttribute staminaAttribute  = metadata.attributes.Find(trait => trait.trait_type.Equals("Stamina"));
            NftAttribute StrengthAttribute = metadata.attributes.Find(trait => trait.trait_type.Equals("Strength"));
            
            string animationUUID = (contract.address + metadata.id);

            ERarity rarity = ERarity.NONE;
                
            if (KinetixBackendAPI.IsContractUGC(contract.address))
                rarity = ERarity.USER_GENERATED;
            else
            {
                rarity = metadata.rarity switch
                {
                    "Common"     => ERarity.COMMON,
                    "Rare"       => ERarity.RARE,
                    "Super Rare" => ERarity.SUPER_RARE,
                    "Unique"     => ERarity.UNIQUE,
                    _            => rarity
                };
            }

            AnimationMetadata animationMetadata = new AnimationMetadata
            {
                Ids          = new AnimationIds(animationUUID, metadata.id.ToString(), contract.address),
                Name         = title,
                Description  = description,
                AnimationURL = metadata.external_url,
                IconeURL     = metadata.thumbnail_url,

                EmoteRarity = rarity,
                
                Exploration = explorationAttribute != null
                    ? new NumberAttribute
                    {
                        Value = Int32.Parse(explorationAttribute.value), Max_Value = explorationAttribute.max_value
                    }
                    : new NumberAttribute()
                    {
                        Value = 0, Max_Value = 100
                    },
                Ranking = rankingAttribute != null
                    ? new NumberAttribute
                    {
                        Value = Int32.Parse(rankingAttribute.value), Max_Value = rankingAttribute.max_value
                    }
                    : new NumberAttribute()
                    {
                        Value = 0, Max_Value = 100
                    },
                ArtisticLevel = artisticLevelAttribute != null
                    ? new NumberAttribute
                    {
                        Value     = Int32.Parse(artisticLevelAttribute.value),
                        Max_Value = artisticLevelAttribute.max_value
                    }
                    : new NumberAttribute()
                    {
                        Value = 0, Max_Value = 100
                    },
                TechnicalLevel = technicalLevelAttribute != null
                    ? new NumberAttribute
                    {
                        Value     = Int32.Parse(technicalLevelAttribute.value),
                        Max_Value = technicalLevelAttribute.max_value
                    }
                    : new NumberAttribute()
                    {
                        Value = 0, Max_Value = 100
                    },
                VisualElement = visualElementAttribute != null
                    ? new ElementAttribute
                    {
                        Value = (EVisualElement)Enum.Parse(typeof(EVisualElement), visualElementAttribute.value,
                            true)
                    }
                    : new ElementAttribute()
                    {
                        Value = EVisualElement.NONE
                    },
                VisualEffect = visualEffectAttribute != null
                    ? new BoolAttribute
                    {
                        Value = !visualEffectAttribute.value.Equals("No")
                    }
                    : new BoolAttribute()
                    {
                        Value = false
                    },
                ContactFeet = contactFeetAttribute != null
                    ? new BoolAttribute
                    {
                        Value = contactFeetAttribute != null ? !contactFeetAttribute.value.Equals("No") : false
                    }
                    : new BoolAttribute()
                    {
                        Value = false
                    },
                Aerial = aerialAttribute != null
                    ? new NumberAttribute
                    {
                        Value = Int32.Parse(aerialAttribute.value), Max_Value = aerialAttribute.max_value
                    }
                    : new NumberAttribute()
                    {
                        Value = 0, Max_Value = 100
                    },
                Amplitude = amplitudeAttribute != null
                    ? new NumberAttribute
                    {
                        Value = Int32.Parse(amplitudeAttribute.value), Max_Value = amplitudeAttribute.max_value
                    }
                    : new NumberAttribute()
                    {
                        Value = 0, Max_Value = 100
                    },
                Speed = speedAttribute != null
                    ? new NumberAttribute
                    {
                        Value = Int32.Parse(speedAttribute.value), Max_Value = speedAttribute.max_value
                    }
                    : new NumberAttribute()
                    {
                        Value = 0, Max_Value = 100
                    },
                Spin = spinAttribute != null
                    ? new NumberAttribute
                    {
                        Value = Int32.Parse(spinAttribute.value), Max_Value = spinAttribute.max_value
                    }
                    : new NumberAttribute()
                    {
                        Value = 0, Max_Value = 100
                    },
                Stamina = staminaAttribute != null
                    ? new NumberAttribute
                    {
                        Value = Int32.Parse(staminaAttribute.value), Max_Value = staminaAttribute.max_value
                    }
                    : new NumberAttribute()
                    {
                        Value = 0, Max_Value = 100
                    },
                Strength = StrengthAttribute != null
                    ? new NumberAttribute
                    {
                        Value = Int32.Parse(StrengthAttribute.value), Max_Value = StrengthAttribute.max_value
                    }
                    : new NumberAttribute()
                    {
                        Value = 0, Max_Value = 100
                    },
            };

            return animationMetadata;
        }

        

        [Serializable]
        public class Contract
        {
            public string address;
        }

        [Serializable]
        public class ContractMetadata
        {
            public string name;
            public string symbol;
            public string tokenType;
        }

        [Serializable]
        public class Id
        {
            public string        tokenId;
            public TokenMetadata tokenMetadata;
        }

        [Serializable]
        public class Medium
        {
            public string raw;
            public string gateway;
        }

        [Serializable]
        public class Metadata
        {
            public string          rarity;
            public string          image;
            public string          external_url;
            public string          animation_url;
            public string          thumbnail_url;
            public string          iframe_url;
            public string          name;
            public string          description;
            public string          animation_uuid;
            public List<NftAttribute> attributes;
            public int             id;
            public List<object>    tags;
        }

        [Serializable]
        public class Root
        {
            public Contract         contract;
            public Id               id;
            public string           title;
            public string           description;
            public TokenUri         tokenUri;
            public List<Medium>     media;
            public Metadata         metadata;
            public DateTime         timeLastUpdated;
            public ContractMetadata contractMetadata;
        }

        [Serializable]
        public class TokenMetadata
        {
            public string tokenType;
        }

        [Serializable]
        public class TokenUri
        {
            public string raw;
            public string gateway;
        }
    }
}

