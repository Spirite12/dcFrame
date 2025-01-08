using System.Collections.Generic;
using DCFrame;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.IO;

public class AddressableEditor : Editor {
    [MenuItem("Tools/资源项/打 AA 包")]
    public static void PackageAddressable() {
        AARules = AssetDatabase.LoadAssetAtPath<AARules>(DCConst.AARulesPath);
        if (AARules == null) {
            Debug.LogError("找不到AA规则文件");
            return;
        }
        StartAddressable();
    }

    private static void StartAddressable() {
        settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        var groupSingle = InitGroupData(DCConst.AAGroupSingle);
        DealWithGroupSingle(groupSingle);
        var groupLabel = InitGroupData(DCConst.AAGroupLabel);
        DealWithGroupLabel(groupLabel);
        var groupFolder = InitGroupData(DCConst.AAGroupFolder);
        DealWithGroupFolder(groupFolder);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 处理组文件夹
    /// </summary>
    private static void DealWithGroupFolder(AddressableAssetGroup targetGroup) {
        foreach (var item in AARules.folderList) {
            // 收集排除队列
            Dictionary<string, bool> excludeDir = new Dictionary<string, bool>();
            // 地址队列
            List<string> pathList = new List<string>();
            
            var path = AssetDatabase.GetAssetPath(item.folderPath);
            if (path == null) {
                Debug.LogError("AARules 内找不到 " + item.folderPath);
                continue;
            }
            
            TraverseDirectories(path, 1, item.number, pathList);
            foreach (var itemTp in item.excludePathList) {
                var pathTp = AssetDatabase.GetAssetPath(itemTp);
                excludeDir.Add(pathTp, true);
            }
            
            // 添加地址组
            foreach (var pathTp in pathList) {
                if (excludeDir.ContainsKey(pathTp)) {
                    continue;
                }
                var guid = AssetDatabase.AssetPathToGUID(pathTp);
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, targetGroup);
                entry.address = pathTp;
            }
        }
    }
    
    /// <summary>
    /// 处理单一文件
    /// </summary>
    private static void DealWithGroupSingle(AddressableAssetGroup targetGroup) {
        void SetEntryInfo(string file, string address) {
            var guid = AssetDatabase.AssetPathToGUID(file);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, targetGroup);
            entry.address = address;
        }
        
        foreach (var item in AARules.singleList) {
            var path = AssetDatabase.GetAssetPath(item);
            if (Directory.Exists(path)) {
                // 处理文件夹下的所有文件
                string[] subDirectories = Directory.GetFiles(path);
                foreach (var subPath in subDirectories) {
                    if (subPath.EndsWith(".meta")) {
                       continue;
                    }
                    SetEntryInfo(subPath, path);
                }
            }else {
                // 处理单个文件
                SetEntryInfo(path, path);
            }
        }
    }
    
    /// <summary>
    /// 处理标签数据
    /// </summary>
    private static void DealWithGroupLabel(AddressableAssetGroup targetGroup) {
        // 清空标签
        foreach (var label in settings.GetLabels()) {
            settings.RemoveLabel(label);
        }
        void SetEntryInfo(string path, string address, string label) {
            var guid = AssetDatabase.AssetPathToGUID(path);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, targetGroup);
            entry.address = address;
            if (!settings.GetLabels().Contains(label)) {
                settings.AddLabel(label);
            }
            entry.SetLabel(label, true, true);
        }
        
        foreach (var item in AARules.labelList) {
            for (int i = 0; i < item.resList.Count; i++) {
                var path = AssetDatabase.GetAssetPath(item.resList[i]);
                if (Directory.Exists(path)) {
                    // 处理文件夹下的所有文件
                    string[] subDirectories = Directory.GetFiles(path);
                    foreach (var subPath in subDirectories) {
                        if (subPath.EndsWith(".meta")) {
                            continue;
                        }
                        SetEntryInfo(subPath, path, item.label);
                    }
                }else {
                    // 处理单个文件
                    SetEntryInfo(path, path, item.label);
                }
            }
        }
    }

    /// <summary>
    /// 处理组数据
    /// </summary>
    private static AddressableAssetGroup InitGroupData(string groupName) {
        AddressableAssetGroup targetGroup = settings.FindGroup(groupName);
        if (targetGroup == null) {
            // 创建组
            targetGroup = settings.CreateGroup(groupName, false, false, false, null);
            var schemaContent = targetGroup.GetSchema<ContentUpdateGroupSchema>();
            if (schemaContent == null) {
                schemaContent = targetGroup.AddSchema<ContentUpdateGroupSchema>();
                schemaContent.StaticContent = false;
            }
            var schemaBundle = targetGroup.GetSchema<BundledAssetGroupSchema>();
            if (schemaBundle == null) {
                schemaBundle = targetGroup.AddSchema<BundledAssetGroupSchema>();
                schemaBundle.BuildPath.SetVariableByName(settings, "Local.BuildPath");
                schemaBundle.LoadPath.SetVariableByName(settings, "Local.LoadPath");
                schemaBundle.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
                schemaBundle.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
            }
        } else {
            // 清空组
            var entries = new List<AddressableAssetEntry>(targetGroup.entries);
            foreach (var entry in entries) {
                settings.RemoveAssetEntry(entry.guid);
            }
        }
        return targetGroup;
    }
    
    /// 递归遍历目录，获取指定级别的子目录。
    /// <param name="currentPath"> 当前路径 </param>
    /// <param name="currentLevel"> 当前层级 </param>
    /// <param name="targetLevel"> 目标层级 </param>
    /// <param name="result"> 存储结果的列表 </param>
    private static void TraverseDirectories(string currentPath, int currentLevel, int targetLevel, List<string> result) {
        // 如果已经到达目标层级，添加当前路径到结果
        if (currentLevel == targetLevel) {
            string path = currentPath.Replace("\\", "/");
            result.Add(path);
            return;
        }
        // 获取当前路径下的所有子目录
        string[] subDirectories = Directory.GetDirectories(currentPath);
        // 递归处理每个子目录
        foreach (string subDir in subDirectories) {
            TraverseDirectories(subDir, currentLevel + 1, targetLevel, result);
        }
    }

    private static AddressableAssetSettings settings;
    private static AARules AARules;
}
