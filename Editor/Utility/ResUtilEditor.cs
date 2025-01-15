using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ResUtilEditor : Editor {
    
    #region 文件夹
    
    [MenuItem("Tools/资源项/删除空文件夹")]
    public static void DeleteEmptyDic() {
        bool isFind = false;
        string[] allDirectories = AssetDatabase.GetAllAssetPaths();
        foreach (var dir in allDirectories) {
            if (AssetDatabase.IsValidFolder(dir) && AssetDatabase.FindAssets("", new[] { dir }).Length == 0) {
                // 删除文件夹
                Directory.Delete(dir);
                string metaFile = dir + ".meta";
                if (File.Exists(metaFile)) {
                    // 删除.meta文件
                    File.Delete(metaFile);
                }
                isFind = true;
            }
        }
        if (isFind) {
            AssetDatabase.Refresh();
        }
        EditorUtility.DisplayDialog("提示弹窗", isFind ? "清理空文件夹成功" : "无空文件夹清理", "关闭");
    }
    
    /// <summary>   
    /// 创建空Txt文件，防止 Unity 空文件夹存在，但又需要
    /// </summary>
    [MenuItem("Assets/Create/Tools/空文件夹占位", false, 80)]
    public static void CreatePlaceholderTxt() {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string filePath = Path.Combine(path, "Placeholder.txt");
        File.Create(filePath).Dispose();
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Assets/Create/Tools/空文件夹占位", true, 80)]
    public static bool CreatePlaceholderTxtTrue() {
        var theSelection = Selection.activeObject;
        var currentPath = AssetDatabase.GetAssetPath(theSelection);
        var isExist = Directory.Exists(currentPath);
        if (!isExist) {
            return false;
        }
        var files = Directory.GetFiles(currentPath);
        var directories = Directory.GetDirectories(currentPath);
        return files.Length == 0 && directories.Length == 0;
    }

    #endregion

    #region 创建 ScriptableObject

    [MenuItem("Assets/Create/Tools/创建 ScriptableObject", false, 80)]
    public static void CreateScriptableObject() {
        var className = Selection.activeObject.name;
        Type type = FindTypeInAssemblies(className);
        if (type == null || !typeof(ScriptableObject).IsAssignableFrom(type)) {
            Debug.LogError($" 类名：'{className}' 没有找到或者不是继承至 ScriptableObject.");
            return;
        }
        var instance = CreateInstance(type);
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path = Path.GetDirectoryName(path);
        if (path != null) {
            string filePath = Path.Combine(path, className + ".asset");
            AssetDatabase.CreateAsset(instance, filePath);
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("Assets/Create/Tools/创建 ScriptableObject", true, 80)]
    public static bool CreateScriptableObjectTrue() {
        var theSelection = Selection.activeObject;
        var currentPath = AssetDatabase.GetAssetPath(theSelection);
        return Path.GetExtension(currentPath) == ".cs";
    }

    #endregion

    #region 通用函数

    // 遍历所有程序集查找类型
    private static Type FindTypeInAssemblies(string className) {
        // 获取当前已加载的所有程序集
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // 遍历程序集寻找匹配的类型
        foreach (var assembly in assemblies) {
            var type = assembly.GetType(className);
            if (type != null) {
                return type;
            }
        }
        return null;
    }

    #endregion
}