using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DCFrame;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AddressableProcessor : AssetPostprocessor {

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        InitData();
        // 资源导入回调
        foreach (string assetPath in importedAssets) {
            CheckAsset(assetPath);
        }
        // 检测移动的资源
        foreach (var assetPath in movedAssets) {
            CheckAsset(assetPath);
        }
    }

    /// <summary>
    /// 检查资源是否可以标记
    /// </summary>
    private static void CheckAsset(string assetPath) {
        var path = Path.GetDirectoryName(assetPath);
        if (path != null) {
            path = path.Replace("\\", "/");
            if (pathDir.TryGetValue(path, out var value)) {
                string fileName = Path.GetFileName(assetPath);
                string pattern = "^" + Regex.Escape(value.searchPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                if (Regex.IsMatch(fileName, pattern)) {
                    MarkAsAddressable(assetPath, value.label);
                }
            }
        }
    }
    
    /// <summary>
    /// 初始化数据
    /// </summary>
    public static void InitData(bool isForce = false) {
        if (AARules == null) {
            AARules = AssetDatabase.LoadAssetAtPath<AARules>(DCConst.AARulesPath);
        }
        if (pathDir != null && !isForce) {
            return;
        }
        pathDir = new Dictionary<string, DirClass>();
        foreach (var single in AARules.singleList) {
            var path = AssetDatabase.GetAssetPath(single.asset);
            if (Directory.Exists(path)) {
                List<string> pathList = new List<string>();
                FileUtil.TraverseDirectories(path, 1, single.directory.number, pathList);
                foreach (var pathTp in pathList) {
                    if (single.directory.option == SearchOption.AllDirectories) {
                        string[] subDirectories = Directory.GetDirectories(pathTp, "*", SearchOption.AllDirectories);
                        var newPath = path.Replace("\\", "/");
                        pathDir.TryAdd(newPath, new DirClass(){searchPattern = single.directory.searchPattern});
                        foreach (var subPath in subDirectories) {
                            newPath = subPath.Replace("\\", "/");
                            pathDir.TryAdd(newPath, new DirClass(){searchPattern = single.directory.searchPattern});
                        }
                    }else {
                        var newPath = pathTp.Replace("\\", "/");
                        pathDir.TryAdd(newPath, new DirClass(){searchPattern = single.directory.searchPattern});
                    }
                }
            }
        }
        foreach (var label in AARules.labelList) {
            foreach (var res in label.resList) {
                var path = AssetDatabase.GetAssetPath(res.asset);
                if (Directory.Exists(path)) {
                    List<string> pathList = new List<string>();
                    FileUtil.TraverseDirectories(path, 1, res.directory.number, pathList);
                    foreach (var pathTp in pathList) {
                        if (res.directory.option == SearchOption.AllDirectories) {
                            string[] subDirectories = Directory.GetDirectories(pathTp, "*", SearchOption.AllDirectories);
                            var newPath = path.Replace("\\", "/");
                            pathDir.TryAdd(newPath, new DirClass(){searchPattern = res.directory.searchPattern});
                            foreach (var subPath in subDirectories) {
                                newPath = subPath.Replace("\\", "/");
                                pathDir.TryAdd(newPath, new DirClass() { searchPattern = res.directory.searchPattern, label = label.label});
                            }
                        }
                        else {
                            var newPath = pathTp.Replace("\\", "/");
                            pathDir.TryAdd(newPath, new DirClass() { searchPattern = res.directory.searchPattern, label = label.label});
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 将资源标记为 Addressable
    /// </summary>
    private static void MarkAsAddressable(string assetPath, string label) {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) {
            Debug.LogError("AddressableAssetSettings 未找到");
            return;
        }
        AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(assetPath));
        if (entry != null) {
            if (entry.address != assetPath) {
                entry.address = assetPath;
            }
            return;
        }
        AddressableAssetGroup defaultGroup = settings.DefaultGroup;
        entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), defaultGroup);
        entry.address = assetPath;
        if (label != "") {
            entry.SetLabel(label, true, true);
        }
    }
    
    private static AARules AARules;
    private static Dictionary<string, DirClass> pathDir;

    private class DirClass {
        public string searchPattern;
        public string label = "";
    }
}
