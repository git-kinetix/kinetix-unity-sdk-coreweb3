using System;
using System.Collections.Generic;

namespace Kinetix.Internal 
{
    [Serializable]
    public class MintingMetadataResult
    {
        public string          rarity;
        public List<NftAttribute> attributes;
        public string          price;
        public string          contractAddress;
        public string          description;
        public int             id;
        public string          name;
        public List<string>    tags;
        public string          image;
        public string          thumbnail_url;
        public string          external_url;
        public string          animation_url;
        public string          iframe_url;
        public DateTime          createdAt;

        public AnimationMetadata ToAnimationMetadata()
        {
            NftAttribute explorationAttribute = attributes.Find(trait => trait.trait_type.Equals("Exploration"));
            NftAttribute rankingAttribute     = attributes.Find(trait => trait.trait_type.Equals("Ranking"));
            NftAttribute artisticLevelAttribute =
                attributes.Find(trait => trait.trait_type.Equals("Artistic level"));
            NftAttribute technicalLevelAttribute =
                attributes.Find(trait => trait.trait_type.Equals("Technical level"));
            NftAttribute visualElementAttribute =
                attributes.Find(trait => trait.trait_type.Equals("Visual Element"));
            NftAttribute visualEffectAttribute =
                attributes.Find(trait => trait.trait_type.Equals("Visual Effect"));
            NftAttribute contactFeetAttribute =
                attributes.Find(trait => trait.trait_type.Equals("Contact Feet"));
            NftAttribute aerialAttribute = attributes.Find(trait => trait.trait_type.Equals("Aerial"));
            NftAttribute amplitudeAttribute =
                attributes.Find(trait => trait.trait_type.Equals("Amplitude"));
            NftAttribute speedAttribute    = attributes.Find(trait => trait.trait_type.Equals("Speed"));
            NftAttribute spinAttribute     = attributes.Find(trait => trait.trait_type.Equals("Spin"));
            NftAttribute staminaAttribute  = attributes.Find(trait => trait.trait_type.Equals("Stamina"));
            NftAttribute StrengthAttribute = attributes.Find(trait => trait.trait_type.Equals("Strength"));

            ERarity enumRarity = ERarity.NONE;
             
            enumRarity = rarity switch
            {
                "Common"     => ERarity.COMMON,
                "Rare"       => ERarity.RARE,
                "Super Rare" => ERarity.SUPER_RARE,
                "Unique"     => ERarity.UNIQUE,
                _            => ERarity.NONE
            };

            AnimationMetadata animationMetadata = new AnimationMetadata
            {
                Ids          = new AnimationIds(contractAddress + id.ToString(), id.ToString(), contractAddress),
                Name         = name,
                Description  = description,
                AnimationURL = external_url,
                IconeURL     = thumbnail_url,
                Ownership = EOwnership.OWNER,
                CreatedAt = createdAt,

                

                EmoteRarity = enumRarity,
                
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

        public Web2EmoteMetadata toWeb2Emote()
        {
            return new Web2EmoteMetadata {
                uuid = id.ToString(),
                name = name,
                origin = string.Empty,
                duration = 0
            };
        }
    }
}
