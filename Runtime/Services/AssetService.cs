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
    public class AssetService: IKinetixService
    {
        private Dictionary<AnimationIds, Sprite>    IconSpritesByIds;
        private Dictionary<AnimationIds, Texture2D> IconTexturesByIds;

        public AssetService()
        {
            IconTexturesByIds                           =  new Dictionary<AnimationIds, Texture2D>();
            IconSpritesByIds                            =  new Dictionary<AnimationIds, Sprite>();
            MonoBehaviourHelper.Instance.OnDestroyEvent += OnDestroy;
        }
        
        public async Task<Sprite> LoadIcon(AnimationIds _Ids, CancellationTokenSource cancelToken = null)
        {
            KinetixEmote emote = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_Ids);
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
                IconDownloaderConfig iconOperationConfig = new IconDownloaderConfig(emote.Metadata.IconeURL);
                IconDownloader iconOperation = new IconDownloader(iconOperationConfig);

                IconDownloaderResponse response = await OperationManagerShortcut.Get().RequestExecution<IconDownloaderConfig, IconDownloaderResponse>(iconOperation, cancelToken);

                tex2D  = response.texture;
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

        public void UnloadIcon(AnimationIds _Ids)
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

        private void OnDestroy()
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
