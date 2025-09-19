using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MiGame.Shop;

namespace MiGame.EditorTools
{
	/// <summary>
	/// 检查商城里配置为迷你币购买的商品：
	/// 1) 价格.迷你币类型 == 迷你币
	/// 2) 校验 价格.迷你币数量 > 0
	/// 3) 校验 特殊属性.迷你商品ID > 0
	/// 4) 校验 是否配置了对应商品（至少配置了一个有效的获得物品，或配置了奖池）
	/// 仅输出关键错误与简要统计。
	/// </summary>
	public static class CheckMiniCoinShopItems
	{
		[MenuItem("Tools/商城/检查迷你币商品配置", priority = 2000)]
		public static void ValidateMiniCoinItems()
		{
			int checkedCount = 0;
			int passedCount = 0;
			int errorCount = 0;
			int skippedCount = 0; // 非迷你币类型的跳过数量

			var failedSummaries = new List<string>();

			// 仅扫描商城目录
			string[] guids = AssetDatabase.FindAssets("t:ShopItemConfig", new[] { "Assets/GameConf/商城" });
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				var config = AssetDatabase.LoadAssetAtPath<ShopItemConfig>(path);
				if (config == null) continue;

				checkedCount++;

				// 仅检查配置了迷你币类型的商品
				bool isMiniCoin = config.价格 != null && config.价格.迷你币类型 == 迷你币类型.迷你币;
				if (!isMiniCoin)
				{
					skippedCount++;
					continue;
				}

				List<string> errors = new List<string>();

				// 校验 迷你币数量
				if (config.价格.迷你币数量 <= 0)
				{
					errors.Add("迷你币数量<=0");
				}

				// 校验 迷你商品ID
				if (config.特殊属性 == null || config.特殊属性.迷你商品ID <= 0)
				{
					errors.Add("迷你商品ID未配置或<=0");
				}

				// 校验是否配置了对应的商品：
				// 至少满足：配置了奖池，或者 获得物品 列表里有有效条目
				bool hasReward = false;
				if (config.奖池 != null)
				{
					hasReward = true;
				}
				else if (config.获得物品 != null && config.获得物品.Count > 0)
				{
					foreach (var r in config.获得物品)
					{
						if (r == null) continue;
						// 物品/伙伴/宠物/翅膀/尾迹 需要 指向的对象不为空；玩家变量/玩家属性 需要 变量名称非空
						bool namedType = r.商品类型 == 商品类型.物品 || r.商品类型 == 商品类型.伙伴 || r.商品类型 == 商品类型.宠物 || r.商品类型 == 商品类型.翅膀 || r.商品类型 == 商品类型.尾迹;
						bool variableType = r.商品类型 == 商品类型.玩家变量 || r.商品类型 == 商品类型.玩家属性;
						if ((namedType && r.商品名称 != null) || (variableType && !string.IsNullOrEmpty(r.变量名称)))
						{
							hasReward = true;
							break;
						}
					}
				}

				if (!hasReward)
				{
					errors.Add("未配置获得物品或奖池");
				}

				if (errors.Count == 0)
				{
					passedCount++;
				}
				else
				{
					errorCount++;
					string errMsg = $"[迷你币校验] 商品'{config.name}' 路径:{path} — 错误: {string.Join("，", errors)}";
					failedSummaries.Add(errMsg);
					Debug.LogError(errMsg);
				}
			}

			Debug.Log($"[迷你币校验] 完成。共检查配置为迷你币的商品 {checkedCount} 个（其中非迷你币类型跳过 {skippedCount} 个），通过 {passedCount} 个，错误 {errorCount} 个。");

			if (failedSummaries.Count > 0)
			{
				Debug.LogError("[迷你币校验] 未通过清单:\n" + string.Join("\n", failedSummaries));
			}
		}
	}
}
