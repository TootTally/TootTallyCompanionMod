using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TootTally.Utils
{
    public static class AssetManager
    {
        public static int coroutineCount;

        //Add your asset names here:
        //Assets are attempted to be loaded locally. If the assets aren't found, it downloads them from the server's assets API link into the Bepinex/Assets folder and tries to reload them.
        public static readonly List<string> requiredAssetNames = new List<string>
        {
            "profile64.png",
            "global64.png",
            "local64.png",
            "DescCapsule.png",
            "GenreCapsule.png",
            "YearCapsule.png",
            "BPMTimeCapsule.png",
            "BackText.png",
            "BackBackground.png",
            "BackOutline.png",
            "BackShadow.png",
            "PlayText.png",
            "PlayBackground.png",
            "PlayOutline.png",
            "PlayShadow.png",
            "RandomOutline.png",
            "RandomIcon.png",
            "RandomBackground.png",
            "ComposerCapsule.png",
            "SongButtonBackground.png",
            "SongButtonOutline.png",
            "SongButtonShadow.png",
            "pointerBG.png",
            "pointerShadow.png",
            "pointerOutline.png",
            "MultiplayerButtonV2.png",
            "CollectButtonV2.png",
        };

        public static Dictionary<string, Texture2D> textureDictionary;

        public static void LoadAssets()
        {
            coroutineCount = 0;
            string assetDir = Path.Combine(Path.GetDirectoryName(Plugin.Instance.Info.Location), "Assets");
            if (!Directory.Exists(assetDir)) Directory.CreateDirectory(assetDir);

            textureDictionary = new Dictionary<string, Texture2D>();

            foreach (string assetName in requiredAssetNames)
            {
                string assetPath = Path.Combine(assetDir, assetName);
                Plugin.Instance.StartCoroutine(TootTallyAPIService.TryLoadingTextureLocal(assetPath, (texture) =>
                {
                    if (texture != null)
                    {
                        textureDictionary.Add(assetName, texture);
                    }
                    else
                        DownloadAssetFromServer("http://cdn.toottally.com/assets/" + assetName, assetDir, assetName);
                }));
            }
        }

        public static void DownloadAssetFromServer(string apiLink, string assetDir, string assetName)
        {
            coroutineCount++;
            Plugin.LogInfo("Downloading asset " + assetName);
            string assetPath = Path.Combine(assetDir, assetName);
            Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadTextureFromServer(apiLink, assetPath, (success) =>
                {
                    ReloadTextureLocal(assetDir, assetName);
                }));
        }

        public static void ReloadTextureLocal(string assetDir, string assetName)
        {
            string assetPath = Path.Combine(assetDir, assetName);
            Plugin.Instance.StartCoroutine(TootTallyAPIService.TryLoadingTextureLocal(assetPath, (texture) =>
            {
                coroutineCount--;
                if (texture != null)
                {
                    Plugin.LogInfo("Asset " + assetName + " Reloaded");
                    textureDictionary.Add(assetName, texture);
                }
                if (coroutineCount == 0)
                {
                    List<string> missingAssetList = GetMissingAssetsName();
                    if (missingAssetList.Count > 0)
                    {
                        Plugin.LogError("Missing Asset(s):");
                        foreach (string missingAsset in missingAssetList)
                            Plugin.LogError("    " + missingAsset);
                    }
                    else
                        Plugin.LogInfo("All Assets Loaded Correctly");
                }
            }));
        }

        public static List<string> GetMissingAssetsName()
        {
            List<string> missingAssetsNames = new List<string>();
            foreach (string assetName in requiredAssetNames)
                if (!textureDictionary.ContainsKey(assetName)) missingAssetsNames.Add(assetName);
            return missingAssetsNames;
        }

        public static Texture2D GetTexture(string assetKey) => textureDictionary[assetKey];
        public static Sprite GetSprite(string assetKey)
        {
            Texture2D texture = textureDictionary[assetKey];
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 300f);
        }
    }
}
