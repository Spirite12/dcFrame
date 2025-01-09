using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DCFrame.Asset {
    public class Asset {
        /// <summary>
        /// 根据地址动态加载资源
        /// </summary>
        public static async UniTask<object> LoadAsset(string address) {
            // 获取扩展名并判断是否支持
            string extension = Path.GetExtension(address).ToLower();
            if (!ExtensionDic.TryGetValue(extension, out Func<string, UniTask<object>> loadFunc)) {
                Debug.LogError($"暂未支持当前后缀名格式： {extension}");
                return null;
            }
            return await loadFunc.Invoke(address);
        }

        #region 加载前缀和函数
        
        /// <summary>
        /// 项目前缀地址
        /// </summary>
        public enum EnumPrefixPath {
            Single = 0,
        }
        
        /// <summary>
        /// 前缀与地址的映射
        /// </summary>
        private static readonly Dictionary<EnumPrefixPath, string> PrefixPathDic = new() {
            { EnumPrefixPath.Single, "Assets/Simple/"},
        };
        
        /// <summary>
        /// 获取贴图加载地址
        /// </summary>
        public static string GetSpritePath(string path, EnumPrefixPath enumPrefix = EnumPrefixPath.Single) {
            if (!PrefixPathDic.TryGetValue(enumPrefix, out string prefixPath)) {
                ErrorPrefixPathTips(enumPrefix);
                return "";
            }
            return Path.Combine(prefixPath, $"{path}.png");;
        }
        
        /// <summary>
        /// 获取文本加载地址
        /// </summary>
        public static string GetTxtPath(string path, EnumPrefixPath enumPrefix = EnumPrefixPath.Single) {
            if (!PrefixPathDic.TryGetValue(enumPrefix, out string prefixPath)) {
                ErrorPrefixPathTips(enumPrefix);
                return "";
            }
            return Path.Combine(prefixPath, $"{path}.txt");;
        }
        
        /// <summary>
        /// 获取预制件加载地址
        /// </summary>
        public static string GetPrefabPath(string path, EnumPrefixPath enumPrefix = EnumPrefixPath.Single) {
            if (!PrefixPathDic.TryGetValue(enumPrefix, out string prefixPath)) {
                ErrorPrefixPathTips(enumPrefix);
                return "";
            }
            return Path.Combine(prefixPath, $"{path}.prefab");;
        }

        private static void ErrorPrefixPathTips(EnumPrefixPath enumPrefix) {
            Debug.LogError($"暂未支持当前前缀枚举： {enumPrefix}");
        }

        #endregion
        
        #region 获取扩展名和函数
        
        /// <summary>
        /// 扩展名与加载函数的映射
        /// </summary>
        private static readonly Dictionary<string, Func<string, UniTask<object>>> ExtensionDic = new(){ 
            { ".png", LoadAssetSprite },
            { ".txt", LoadAssetTxt },
            { ".prefab", LoadAssetPrefab },
        };

        /// <summary>
        /// 下载 预制件 类型资源
        /// </summary>
        private static async UniTask<object> LoadAssetPrefab(string address) {
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            await handle.Task;
            if (handle.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogError($"加载 Prefab 失败，地址是: {address}");
                return null;
            }
            return handle.Result;
        }

        /// <summary>
        /// 下载 文本 类型资源
        /// </summary>
        private static async UniTask<object> LoadAssetTxt(string address) {
            var handle = Addressables.LoadAssetAsync<TextAsset>(address);
            await handle.Task;
            if (handle.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogError($"加载 Txt 失败，地址是: {address}");
                return null;
            }
            return handle.Result;
        }

        /// <summary>
        /// 加载 Sprite 类型资源
        /// </summary>
        private static async UniTask<object> LoadAssetSprite(string address) {
            var handle = Addressables.LoadAssetAsync<Sprite>(address);
            await handle.Task;
            if (handle.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogError($"加载 Sprite 失败，地址是: {address}");
                return null;
            }
            return handle.Result;
        }

        #endregion
    }
}
