using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DCFrame {
    public class AARules : ScriptableObject {
    
        [Header("以单资源打包（可以是资源、文件夹）")]
        public List<HierarchySingle> singleList = new List<HierarchySingle>();
    
        [Header("以标签名打包（可以是资源、文件夹）")]
        public List<HierarchyLabel> labelList = new List<HierarchyLabel>();
    
        [Header("以文件夹打包（文件夹之间没有关联）")]
        public List<HierarchyDir> folderList = new List<HierarchyDir>();
    
        [System.Serializable]
        public class HierarchySingle {
            public Object asset;
            public HierarchySingleDir directory;
        }
    
        [System.Serializable]
        public class HierarchySingleDir {
            [Range(1, 5)]
            public int number = 1;
            public string searchPattern = "*.*";
            public SearchOption option = SearchOption.TopDirectoryOnly;
        }
    
        [System.Serializable]
        public class HierarchyLabel {
            public string label = "";
            public List<HierarchySingle> resList = new List<HierarchySingle>();
        }        
        
        [System.Serializable]
        public class HierarchyDir {
            public Object folderPath;
            [Range(1, 5)]
            [Header("第几级文件夹")]
            public int number = 1;
            [Header("过滤的文件夹")]
            public List<Object> excludePathList = new List<Object>();
        }
    }
}
