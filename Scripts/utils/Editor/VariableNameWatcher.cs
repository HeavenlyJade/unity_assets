using UnityEditor;
using System.Linq;

namespace MiGame.Utils.Editor
{
    public class VariableNameWatcher : AssetPostprocessor
    {
        // 当任何资源被修改、导入或移动后，这个方法会被Unity自动调用
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // 我们只关心 VariableNames.json 文件的变化
            const string targetFile = "Assets/GameConf/玩家变量/VariableNames.json";

            // 检查是否有任何被导入/修改的资源是我们的目标文件
            if (importedAssets.Any(path => path.Equals(targetFile)))
            {
                // 如果是，就通知 携带效果Drawer 清除它的缓存
                // 下次绘制时，它就会重新加载JSON文件
                携带效果Drawer.InvalidateCache();
            }
        }
    }
}
