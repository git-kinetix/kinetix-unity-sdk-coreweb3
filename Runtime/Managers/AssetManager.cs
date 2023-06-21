using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Kinetix.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kinetix.Internal
{
    public static class AssetManager
    {
        private static Dictionary<AnimationIds, Sprite>    IconSpritesByIds;
        private static Dictionary<AnimationIds, Texture2D> IconTexturesByIds;

        public static void Initialize()
        {
            IconTexturesByIds                           =  new Dictionary<AnimationIds, Texture2D>();
            IconSpritesByIds                            =  new Dictionary<AnimationIds, Sprite>();
            MonoBehaviourHelper.Instance.OnDestroyEvent += OnDestroy;
        }
        
        public static async Task<Sprite> LoadIcon(AnimationIds _Ids, TokenCancel cancelToken = null)
        {
            if (IconTexturesByIds == null)
                Initialize();

            KinetixEmote emote = EmotesManager.GetEmote(_Ids);
            if (!emote.HasMetadata())
                return null;
            if (string.IsNullOrEmpty(emote.Metadata.IconeURL))
                return null;

            if (IconSpritesByIds.ContainsKey(_Ids))
                return IconSpritesByIds[_Ids];

            Sprite    sprite = null;
            Texture2D tex2D  = null;
            
            try
            {
                tex2D  = await IconOperationManager.DownloadTexture(emote, cancelToken);
                sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100f, 0, SpriteMeshType.FullRect);
                sprite.name = "Icon_" + emote.Metadata.Name;
            }
            catch (TaskCanceledException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                if (IconTexturesByIds != null && !IconTexturesByIds.ContainsKey(_Ids))
                    IconTexturesByIds.Add(_Ids, null);
            
                if (IconSpritesByIds != null && !IconSpritesByIds.ContainsKey(_Ids))
                    IconSpritesByIds.Add(_Ids, null);
                throw e;
            }
            
            if (IconTexturesByIds != null && !IconTexturesByIds.ContainsKey(_Ids))
                IconTexturesByIds.Add(_Ids, tex2D);
            
            if (IconSpritesByIds != null && !IconSpritesByIds.ContainsKey(_Ids))
                IconSpritesByIds.Add(_Ids, sprite);

            return sprite;
        }

        public static void UnloadIcon(AnimationIds _Ids)
        {
            if (IconTexturesByIds.ContainsKey(_Ids))
            {
                Texture2D tex = IconTexturesByIds[_Ids];
                Object.DestroyImmediate(tex);
                IconTexturesByIds.Remove(_Ids);
            }
            
            if (IconSpritesByIds.ContainsKey(_Ids))
            {
                Sprite sprite = IconSpritesByIds[_Ids];
                Object.Destroy(sprite);
                IconSpritesByIds.Remove(_Ids);
            }
        }

        private static void OnDestroy()
        {
            foreach (Sprite sprite in IconSpritesByIds.Values)
            {
                Object.DestroyImmediate(sprite);
            }
            
            foreach (Texture2D tex2D in IconTexturesByIds.Values)
            {
                Object.DestroyImmediate(tex2D);
            }
        }
        
 
    }
}
