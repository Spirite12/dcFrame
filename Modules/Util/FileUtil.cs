using System.Collections.Generic;
using System.IO;

namespace DCFrame {
    public class FileUtil {
        /// 递归遍历目录，获取指定级别的子目录。
        /// <param name="currentPath"> 当前路径 </param>
        /// <param name="currentLevel"> 当前层级 </param>
        /// <param name="targetLevel"> 目标层级 </param>
        /// <param name="result"> 存储结果的列表 </param>
        public static void TraverseDirectories(string currentPath, int currentLevel, int targetLevel, List<string> result) {
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
    }
}