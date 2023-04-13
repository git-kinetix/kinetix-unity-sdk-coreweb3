using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Kinetix.Internal
{
    [Serializable]
    public class FreeAnimationManifest
    {
        [UnityEngine.SerializeField]
        public List<FreeAnimationPath> emotesPaths;

        private string path;

        public FreeAnimationManifest(string path)
        {
            this.path = path;
            emotesPaths = new List<FreeAnimationPath>();
        }

        public void RemoveWhenNotInList(List<string> emoteNames, string path)
        {
            List<FreeAnimationPath> toRemove = new List<FreeAnimationPath>();

            foreach (FreeAnimationPath emotePath in emotesPaths) {
                if (emotePath.emoteLocation == path && !emoteNames.Contains(emotePath.emoteName)) {
                    toRemove.Add(emotePath);
                }
            }

            foreach (FreeAnimationPath emotePath in toRemove) {
                emotesPaths.Remove(emotePath);
            }
        }
        
        public bool Contains(string emoteName, string path)
        {
            foreach (FreeAnimationPath emotePath in emotesPaths)
            {
                if (emotePath.emoteName == emoteName && emotePath.emoteLocation == path) return true;
            }

            return false;
        }

        public void Save()
        {
            StreamWriter fileWriter = new StreamWriter(path, false);
            fileWriter.WriteLine(JsonConvert.SerializeObject(this));
            fileWriter.Close();
        }

        public void RemoveEmote(string emoteName)
        {
            for (int i = 0; i < emotesPaths.Count; i++)
            {
                if (emotesPaths[i].emoteName == emoteName) {
                    emotesPaths.RemoveAt(i);
                    break;
                }
            }
        }

        public void RemoveEmote(string path, string emoteName)
        {
            for (int i = 0; i < emotesPaths.Count; i++)
            {
                if (emotesPaths[i].emoteName == emoteName && emotesPaths[i].emoteLocation == path) {
                    emotesPaths.RemoveAt(i);
                    break;
                }
            }
        }

        public void AddEmote(string path, string emoteName)
        {
            emotesPaths.Add(new FreeAnimationPath(emoteName, path));
        }

        public void SetPath(string path)
        {
            this.path = path;
        }

        public static FreeAnimationManifest LoadFromPath(string path)
        {

            UnityEngine.Debug.Log(path);
            if (!File.Exists(path)) return new FreeAnimationManifest(path);
            
            var sr = new StreamReader(path);
            string json = sr.ReadToEnd();
            sr.Close();
            
            FreeAnimationManifest manifest = JsonConvert.DeserializeObject<FreeAnimationManifest>(json);
            manifest.SetPath(path);

            return manifest;
        }
    }

    [Serializable]
    public class FreeAnimationPath
    {
        public string emoteName;
        public string emoteLocation;

        public FreeAnimationPath(string emoteName, string emoteLocation)
        {
            this.emoteName = emoteName;
            this.emoteLocation = emoteLocation;
        }
    }
}
