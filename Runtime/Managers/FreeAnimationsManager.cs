// // ----------------------------------------------------------------------------
// // <copyright file="CacheManager.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kinetix.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Kinetix.Internal
{
    internal static class FreeAnimationsManager
    {
        
        private static KinetixEmote[] FreeEmotes;
        private static TaskCompletionSource<KinetixEmote[]> tcs;

        public static void Initialize()
        {
            FreeEmotes     ??= Array.Empty<KinetixEmote>();
        }

        public static async Task<KinetixEmote[]> GetFreeEmotes()
        {
            if (FreeEmotes != null && FreeEmotes.Length > 0)
                return FreeEmotes;
            
            if (tcs != null)
                return await tcs.Task;
            
            tcs = new TaskCompletionSource<KinetixEmote[]>();

            string manifestPath = Path.Combine(Application.streamingAssetsPath, KinetixConstants.C_FreeAnimationsManifestPath);
            string manifestContent = await WebRequestHandler.Instance.GetAsyncRaw(Application.streamingAssetsPath + "/Kinetix/FreeEmoteManifest.json", null);

            if (manifestContent == string.Empty) {
                FreeEmotes = new KinetixEmote[0];
                
                tcs.SetResult(FreeEmotes);
            }

            FreeAnimationManifest manifest = JsonConvert.DeserializeObject<FreeAnimationManifest>(manifestContent);
            
            List<KinetixEmote> emotes = await GetFreeEmoteFromManifest(manifest);

            FreeEmotes = emotes.ToArray();
            tcs.SetResult(FreeEmotes);
            
            return await tcs.Task;
        }

        private static async Task<List<KinetixEmote>> GetFreeEmoteFromManifest(FreeAnimationManifest manifest)
        {
            List<KinetixEmote> emotes = new List<KinetixEmote>();

            foreach (FreeAnimationPath emotePath in manifest.emotesPaths) {
                string filePathName = Path.Combine(Application.streamingAssetsPath, emotePath.emoteLocation, emotePath.emoteName, emotePath.emoteName);
                AnimationMetadata emoteMetadata = null;

                string json = await WebRequestHandler.Instance.GetAsyncRaw(filePathName + ".json", null);

                if (json == string.Empty) {
                    continue;
                }

                emoteMetadata = JsonConvert.DeserializeObject<Web2EmoteMetadata>(json).ToAnimationMetadata();

                emoteMetadata.IconeURL = filePathName + ".png";
                KinetixEmote emote = EmotesManager.GetEmote(emoteMetadata.Ids);
                //Sprite emoteIcone = await AssetManager.LoadIcon(filePathName + ".png");
                //emoteMetadata.SetLocalIcon(emoteIcone);
                
                emote.SetLocalMetadata(emoteMetadata, filePathName + ".glb");
                emotes.Add(emote);
            }

            return emotes;
        }


        public static async void AddFreeAnimations(Action _OnSuccess)
        {            
            try
            {
                await GetFreeEmotes();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                // ignored
            }

            _OnSuccess?.Invoke();
        }
    }
}
