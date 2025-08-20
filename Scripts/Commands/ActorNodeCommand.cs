using UnityEngine;

namespace MiGame.Commands
{
    /// <summary>
    /// 节点操作类型枚举
    /// </summary>
    public enum NodeOperationType
    {
        设置可见性    // 设置节点可见性
    }

    /// <summary>
    /// 玩家对象节点控制指令
    /// 用于控制玩家UI节点的可见性
    /// </summary>
    [Command("actornode", "用于控制玩家对象节点可见性的指令")]
    public class ActorNodeCommand : AbstractCommand
    {
        [Header("基础参数")]
        [Tooltip("目标玩家UID")]
        public string 玩家UID;

        [Tooltip("操作类型")]
        public NodeOperationType 操作类型;

        [Tooltip("目标节点名称（支持路径格式，如：称号/特权/图标）")]
        public string 节点名称;

        [Tooltip("节点可见性")]
        public bool 可见性;

        public override void Execute()
        {
            // 基础参数验证
            if (string.IsNullOrEmpty(节点名称))
            {
                Debug.LogError("节点名称 不能为空");
                return;
            }

            // 查找并设置节点可见性
            SetNodeVisibility();
        }

        /// <summary>
        /// 设置节点可见性
        /// </summary>
        private void SetNodeVisibility()
        {
            // 查找目标节点
            GameObject targetNode = FindNodeByPath(节点名称);
            if (targetNode == null)
            {
                Debug.LogError($"未找到节点: {节点名称}");
                return;
            }

            // 设置可见性
            var canvasGroup = targetNode.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 可见性 ? 1f : 0f;
                canvasGroup.interactable = 可见性;
                canvasGroup.blocksRaycasts = 可见性;
            }
            else
            {
                // 如果没有CanvasGroup组件，尝试设置Renderer的enabled
                var renderer = targetNode.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = 可见性;
                }
                else
                {
                    // 最后尝试设置GameObject的active状态
                    targetNode.SetActive(可见性);
                }
            }

            Debug.Log($"设置节点 {节点名称} 可见性为: {可见性}");
        }

        /// <summary>
        /// 根据路径查找节点
        /// </summary>
        private GameObject FindNodeByPath(string nodePath)
        {
            if (string.IsNullOrEmpty(nodePath))
                return null;

            // 分割路径
            string[] pathParts = nodePath.Split('/');
            if (pathParts.Length == 0)
                return null;

            // 查找根节点
            GameObject rootNode = GameObject.Find(pathParts[0]);
            if (rootNode == null)
                return null;

            // 如果只有一个路径部分，直接返回
            if (pathParts.Length == 1)
                return rootNode;

            // 遍历路径查找子节点
            Transform currentTransform = rootNode.transform;
            for (int i = 1; i < pathParts.Length; i++)
            {
                Transform childTransform = currentTransform.Find(pathParts[i]);
                if (childTransform == null)
                {
                    Debug.LogWarning($"路径 {nodePath} 中找不到子节点: {pathParts[i]}");
                    return null;
                }
                currentTransform = childTransform;
            }

            return currentTransform.gameObject;
        }
    }
}
