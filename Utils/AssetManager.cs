﻿using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        };

        public static Dictionary<string, Texture2D> textureDictionary;

        public static void LoadAssets()
        {
            coroutineCount = 0;
            string assetDir = Path.Combine(Paths.BepInExRootPath, "Assets/");
            if (!Directory.Exists(assetDir)) Directory.CreateDirectory(assetDir);

            textureDictionary = new Dictionary<string, Texture2D>();

            foreach (string assetName in requiredAssetNames)
            {
                Plugin.Instance.StartCoroutine(TootTallyAPIService.TryLoadingTextureLocal(assetDir + assetName, (texture) =>
                {
                    if (texture != null)
                    {
                        Plugin.LogInfo("Asset " + assetName + " Loaded");
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
            Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadTextureFromServer(apiLink, assetDir + assetName, (success) =>
                {
                    coroutineCount--;
                    if (success)
                        ReloadTextureLocal(assetDir, assetName);
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

        public static void ReloadTextureLocal(string assetDir, string assetName)
        {
            Plugin.Instance.StartCoroutine(TootTallyAPIService.TryLoadingTextureLocal(assetDir + assetName, (texture) =>
            {
                if (texture != null)
                {
                    Plugin.LogInfo("Asset " + assetName + " Reloaded");
                    textureDictionary.Add(assetName, texture);
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
    }
}