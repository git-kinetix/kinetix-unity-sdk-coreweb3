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
        private static Dictionary<string, Sprite>  IconSpritesByURL;
        private static Dictionary<string, Texture2D> IconTexturesByURL;

        public static void Initialize()
        {
            IconTexturesByURL                           = new Dictionary<string, Texture2D>();
            IconSpritesByURL                            = new Dictionary<string, Sprite>();
            MonoBehaviourHelper.Instance.OnDestroyEvent +=  OnDestroy;
        }
        
        public static async Task<Sprite> LoadIcon(string _URL, TokenCancel cancelToken = null)
        {
            if (IconTexturesByURL == null)
                Initialize();

            if(string.IsNullOrEmpty(_URL))
                return null;

            if (IconSpritesByURL.ContainsKey(_URL))
                return IconSpritesByURL[_URL];

            Sprite    sprite = null;
            Texture2D tex2D  = null;
            
            try
            {
                tex2D  = await IconOperationManager.DownloadTexture(_URL, cancelToken);
                sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100f, 0, SpriteMeshType.FullRect);
            }
            catch (TaskCanceledException e)
            {
                throw e;
            }
            catch (Exception)
            {
                // Let texture and sprite to null if an exception is thrown
            }
            
            if (IconTexturesByURL != null && !IconTexturesByURL.ContainsKey(_URL))
                IconTexturesByURL.Add(_URL, tex2D);
            
            if (IconSpritesByURL != null && !IconSpritesByURL.ContainsKey(_URL))
                IconSpritesByURL.Add(_URL, sprite);

            return sprite;
        }

        public static void UnloadIcon(string _URL)
        {
            if (IconTexturesByURL.ContainsKey(_URL))
            {
                Texture2D tex = IconTexturesByURL[_URL];
                Object.DestroyImmediate(tex);
                IconTexturesByURL.Remove(_URL);
            }
            
            if (IconSpritesByURL.ContainsKey(_URL))
            {
                Sprite sprite = IconSpritesByURL[_URL];
                Object.Destroy(sprite);
                IconSpritesByURL.Remove(_URL);
            }
        }

        private static void OnDestroy()
        {
            foreach (Sprite sprite in IconSpritesByURL.Values)
            {
                Object.DestroyImmediate(sprite);
            }
            
            foreach (Texture2D tex2D in IconTexturesByURL.Values)
            {
                Object.DestroyImmediate(tex2D);
            }
        }
        
 
    }
}
