// FILE_WEB3
using System;
using System.Collections.Generic;
using Kinetix.Internal;

namespace Kinetix.Utils.Alchemy
{
    [Serializable]
    public class GetNftsForOwner : WebRequestHandler.JsonWebResponse
    {
        public List<OwnedNft> ownedNfts;
        public int            totalCount;
        public string         blockHash;
        public string         pageKey;

        public AnimationMetadata[] Deserialize()
        {
            List<AnimationMetadata> animationsMetadata = new List<AnimationMetadata>();

            ownedNfts.ForEach(nft =>
            {
                Attribute explorationAttribute =
                    nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Exploration"));
                Attribute rankingAttribute = nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Ranking"));
                Attribute artisticLevelAttribute =
                    nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Artistic level"));
                Attribute technicalLevelAttribute =
                    nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Technical level"));
                Attribute visualElementAttribute =
                    nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Visual Element"));
                Attribute visualEffectAttribute =
                    nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Visual Effect"));
                Attribute contactFeetAttribute =
                    nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Contact Feet"));
                Attribute aerialAttribute = nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Aerial"));
                Attribute amplitudeAttribute =
                    nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Amplitude"));
                Attribute speedAttribute   = nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Speed"));
                Attribute spinAttribute    = nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Spin"));
                Attribute staminaAttribute = nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Stamina"));
                Attribute StrengthAttribute =
                    nft.metadata.attributes.Find(trait => trait.trait_type.Equals("Strength"));

                string animationUUID = (nft.contract.address + nft.metadata.id);
                
                ERarity rarity = ERarity.NONE;
                
                if (KinetixBackendAPI.IsContractUGC(nft.contract.address))
                    rarity = ERarity.USER_GENERATED;
                else
                {
                    rarity = nft.metadata.rarity switch
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
                    Ownership    = EOwnership.OWNER,
                    Ids          = new AnimationIds(animationUUID, nft.metadata.id.ToString(), nft.contract.address),
                    Name         = nft.title ?? "Title",
                    Description  = nft.description ?? "Description",
                    AnimationURL = nft.metadata.external_url ?? "",
                    IconeURL     = nft.metadata.thumbnail_url ?? "",

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

                animationsMetadata.Add(animationMetadata);
            });

            return animationsMetadata.ToArray();
        }

        public bool HasNextPage()
        {
            return !string.IsNullOrEmpty(pageKey) && pageKey != "";
        }

        public string GetNextPageKey()
        {
            return pageKey;
        }

        [Serializable]
        public class Attribute
        {
            public string value;
            public int    max_value;
            public string trait_type;
            public string display_type;
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
            public string          thumbnail_url;
            public string          animation_url;
            public string          iframe_url;
            public string          name;
            public object          description;
            public string          animation_uuid;
            public List<Attribute> attributes;
            public int             id;
            public object          tags;
        }

        [Serializable]
        public class OwnedNft
        {
            public Contract         contract;
            public Id               id;
            public string           balance;
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
