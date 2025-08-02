using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace MiGame.Utils
{
    /// <summary>
    /// 配置读取工具类
    /// </summary>
    public static class ConfigReader
    {
        /// <summary>
        /// 加成计算方式配置
        /// </summary>
        [System.Serializable]
        public class BonusCalculationMethodConfig
        {
            public string id;
            public string name;
            public string description;
        }

        /// <summary>
        /// 加成计算方式配置容器
        /// </summary>
        [System.Serializable]
        public class BonusCalculationMethodsContainer
        {
            public List<BonusCalculationMethodConfig> BonusCalculationMethods;
        }

        private static BonusCalculationMethodsContainer _bonusCalculationMethods;

        /// <summary>
        /// 获取加成计算方式配置
        /// </summary>
        public static List<BonusCalculationMethodConfig> GetBonusCalculationMethods()
        {
            if (_bonusCalculationMethods == null)
            {
                LoadBonusCalculationMethods();
            }
            return _bonusCalculationMethods?.BonusCalculationMethods ?? new List<BonusCalculationMethodConfig>();
        }

        /// <summary>
        /// 加载加成计算方式配置
        /// </summary>
        private static void LoadBonusCalculationMethods()
        {
            try
            {
                string jsonPath = Path.Combine(Application.dataPath, "Scripts/公共配置/BonusCalculationMethods.json");
                if (File.Exists(jsonPath))
                {
                    string jsonContent = File.ReadAllText(jsonPath);
                    _bonusCalculationMethods = JsonUtility.FromJson<BonusCalculationMethodsContainer>(jsonContent);
                }
                else
                {
                    Debug.LogWarning($"配置文件不存在: {jsonPath}");
                    _bonusCalculationMethods = new BonusCalculationMethodsContainer
                    {
                        BonusCalculationMethods = new List<BonusCalculationMethodConfig>
                        {
                            new BonusCalculationMethodConfig { id = "单独相加", name = "单独相加", description = "将加成值直接累加" },
                            new BonusCalculationMethodConfig { id = "最终乘法", name = "最终乘法", description = "计算的值在最后的结果相乘" },
                            new BonusCalculationMethodConfig { id = "最终相加", name = "最终相加", description = "将加成值（通常是百分比）相加" }
                        }
                    };
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载加成计算方式配置失败: {e.Message}");
                _bonusCalculationMethods = new BonusCalculationMethodsContainer
                {
                    BonusCalculationMethods = new List<BonusCalculationMethodConfig>()
                };
            }
        }

        /// <summary>
        /// 根据ID获取加成计算方式配置
        /// </summary>
        public static BonusCalculationMethodConfig GetBonusCalculationMethodById(string id)
        {
            var methods = GetBonusCalculationMethods();
            return methods.Find(m => m.id == id);
        }

        /// <summary>
        /// 获取所有加成计算方式的ID列表
        /// </summary>
        public static List<string> GetBonusCalculationMethodIds()
        {
            var methods = GetBonusCalculationMethods();
            var ids = new List<string>();
            foreach (var method in methods)
            {
                ids.Add(method.id);
            }
            return ids;
        }


    }
} 