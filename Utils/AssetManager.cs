using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TootTally.Utils
{
    public static class AssetManager
    {
        public static int coroutineCount;
        public const string DEFAULT_TEXTURE_NAME = "icon.png";

        //Add your asset names here:
        //Assets are attempted to be loaded locally. If the assets aren't found, it downloads them from the server's assets API link into the Bepinex/Assets folder and tries to reload them.
        public static readonly List<string> requiredAssetNames = new List<string>
        {
            "icon.png",
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
            "MultiText.png",
            "CollectButtonOutline.png",
            "Close64.png",
            "Download64.png",
            "Check64.png",
            "Block64.png",
            "Twitch64.png",
            "PfpMask.png",
            "Cool-sss.png",
            "HD.png",
            "FL.png",
            "BT.png",
            "FLMask.png",
            "ModifierButton.png",
            "glow.png",
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
                Plugin.Instance.StartCoroutine(TootTallyAPIService.TryLoadingTextureLocal(assetPath, texture =>
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
            TootTallyLogger.LogInfo("Downloading asset " + assetName);
            string assetPath = Path.Combine(assetDir, assetName);
            Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadTextureFromServer(apiLink, assetPath, success =>
                {
                    ReloadTextureLocal(assetDir, assetName);
                }));
        }

        public static void ReloadTextureLocal(string assetDir, string assetName)
        {
            string assetPath = Path.Combine(assetDir, assetName);
            Plugin.Instance.StartCoroutine(TootTallyAPIService.TryLoadingTextureLocal(assetPath, texture =>
            {
                coroutineCount--;
                if (texture != null)
                {
                    TootTallyLogger.LogInfo("Asset " + assetName + " Reloaded");
                    textureDictionary.Add(assetName, texture);
                }
                if (coroutineCount == 0)
                {
                    List<string> missingAssetList = GetMissingAssetsName();
                    if (missingAssetList.Count > 0)
                    {
                        TootTallyLogger.LogError("Missing Asset(s):");
                        foreach (string missingAsset in missingAssetList)
                            TootTallyLogger.LogError("    " + missingAsset);
                    }
                    else
                        TootTallyLogger.LogInfo("All Assets Loaded Correctly");

                }
            }));
        }

        public static void GetProfilePictureByID(int userID, Action<Sprite> callback)
        {
            if (!textureDictionary.ContainsKey(userID.ToString()))
            {
                Plugin.Instance.StartCoroutine(TootTallyAPIService.LoadPFPFromServer(userID, (texture) =>
                {
                    textureDictionary.Add(userID.ToString(), texture);
                    callback(GetSprite(userID.ToString()));
                }));
            }
            else
                callback(GetSprite(userID.ToString()));

        }

        public static List<string> GetMissingAssetsName()
        {
            List<string> missingAssetsNames = new List<string>();
            foreach (string assetName in requiredAssetNames)
                if (!textureDictionary.ContainsKey(assetName)) missingAssetsNames.Add(assetName);
            return missingAssetsNames;
        }

        public static Texture2D GetTexture(string assetKey)
        {
            try
            {
                return textureDictionary[assetKey];
            }
            catch (Exception ex)
            {
                TootTallyLogger.LogError($"Key {assetKey} not found.");
                TootTallyLogger.CatchError(ex);
                return textureDictionary[DEFAULT_TEXTURE_NAME];
            }
        }
        public static Sprite GetSprite(string assetKey)
        {
            try
            {
                Texture2D texture = textureDictionary[assetKey];
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 300f);
            }
            catch (Exception ex)
            {
                TootTallyLogger.LogError($"Key {assetKey} not found.");
                TootTallyLogger.CatchError(ex);
                Texture2D texture = textureDictionary[DEFAULT_TEXTURE_NAME];
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 300f);
            }
        }
    }
}
