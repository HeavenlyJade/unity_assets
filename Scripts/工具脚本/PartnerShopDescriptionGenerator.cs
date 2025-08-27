using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using MiGame.Shop;

namespace MiGame.EditorTools
{
	/// <summary>
	/// 伙伴商城商品描述批量生成工具
	/// 从 Scripts/配置exel/伙伴.json 读取伙伴的加成配置，匹配 GameConf/商城/伙伴商城 下的 ShopItemConfig，生成商品描述
	/// 规则：
	/// 商品描述 = "金币获取：S_LVL*{金币系数}；训练加成：S_LVL*{训练系数}"
	/// </summary>
	public static class PartnerShopDescriptionGenerator
	{
		private const string PartnerJsonPath = "Assets/Scripts/配置exel/伙伴.json";
		private const string PartnerShopFolder = "Assets/GameConf/商城/伙伴商城";
		private const string WingJsonPath = "Assets/Scripts/配置exel/翅膀.json";
		private const string WingShopFolder = "Assets/GameConf/商城/翅膀商城";

		/// <summary>
		/// 菜单入口：工具/商城/生成伙伴商品描述
		/// </summary>
		[MenuItem("Tools/商城/生成伙伴商品描述", false, 2001)]
		public static void GenerateDescriptions()
		{
			try
			{
				if (!File.Exists(PartnerJsonPath))
				{
					Debug.LogError($"未找到伙伴配置文件：{PartnerJsonPath}");
					return;
				}

				string json = File.ReadAllText(PartnerJsonPath, Encoding.UTF8);
				var partnerMap = ParsePartnerBoosts(json);
				if (partnerMap.Count == 0)
				{
					Debug.LogError("解析伙伴配置失败或没有有效数据");
					return;
				}

				string[] guids = AssetDatabase.FindAssets("t:ShopItemConfig", new[] { PartnerShopFolder });
				int updated = 0;
				foreach (string guid in guids)
				{
					string path = AssetDatabase.GUIDToAssetPath(guid);
					var config = AssetDatabase.LoadAssetAtPath<ShopItemConfig>(path);
					if (config == null) continue;

					string name = config.商品名;
					if (string.IsNullOrEmpty(name))
					{
						// 尝试用文件名匹配
						name = Path.GetFileNameWithoutExtension(path);
					}

					if (!string.IsNullOrEmpty(name) && partnerMap.TryGetValue(name, out var boosts))
					{
						string goldMul = ExtractMultiplier(boosts.goldExpr);
						string trainMul = ExtractMultiplier(boosts.trainExpr);
						string desc = $"金币获取：星级X{goldMul}\n训练加成：星级X{trainMul}";

						if (config.商品描述 != desc)
						{
							Undo.RecordObject(config, "更新伙伴商品描述");
							config.商品描述 = desc;
							EditorUtility.SetDirty(config);
							updated++;
						}
					}
					else
					{
						// 未匹配到的项仅提示，不报错
						// Debug.LogWarning($"未在伙伴配置中找到：{name}");
					}
				}

				if (updated > 0)
				{
					AssetDatabase.SaveAssets();
				}
				Debug.Log($"伙伴商城描述生成完成，更新数量：{updated}");
			}
			catch (Exception e)
			{
				Debug.LogError($"生成伙伴商品描述失败：{e.Message}\n{e.StackTrace}");
			}
		}

		/// <summary>
		/// 菜单入口：生成翅膀商品描述
		/// </summary>
		[MenuItem("Tools/商城/生成翅膀商品描述", false, 2002)]
		public static void GenerateWingDescriptions()
		{
			try
			{
				if (!File.Exists(WingJsonPath))
				{
					Debug.LogError($"未找到翅膀配置文件：{WingJsonPath}");
					return;
				}

				string json = File.ReadAllText(WingJsonPath, Encoding.UTF8);
				var wingMap = ParseWingBoosts(json);
				if (wingMap.Count == 0)
				{
					Debug.LogError("解析翅膀配置失败或没有有效数据");
					return;
				}

				string[] guids = AssetDatabase.FindAssets("t:ShopItemConfig", new[] { WingShopFolder });
				int updated = 0;
				foreach (string guid in guids)
				{
					string path = AssetDatabase.GUIDToAssetPath(guid);
					var config = AssetDatabase.LoadAssetAtPath<ShopItemConfig>(path);
					if (config == null) continue;

					string name = config.商品名;
					if (string.IsNullOrEmpty(name))
					{
						name = Path.GetFileNameWithoutExtension(path);
					}

					if (!string.IsNullOrEmpty(name) && wingMap.TryGetValue(name, out var boosts))
					{
						string speed = ExtractMultiplier(boosts.speedExpr);
						string accel = ExtractMultiplier(boosts.accelExpr);
						string gold = ExtractMultiplier(boosts.goldExpr);
						string desc = $"速度加成：星级X{speed}\n加速度：星级X{accel}\n金币加成：星级X{gold}";

						if (config.商品描述 != desc)
						{
							Undo.RecordObject(config, "更新翅膀商品描述");
							config.商品描述 = desc;
							EditorUtility.SetDirty(config);
							updated++;
						}
					}
				}

				if (updated > 0)
				{
					AssetDatabase.SaveAssets();
				}
				Debug.Log($"翅膀商城描述生成完成，更新数量：{updated}");
			}
			catch (Exception e)
			{
				Debug.LogError($"生成翅膀商品描述失败：{e.Message}\n{e.StackTrace}");
			}
		}

		/// <summary>
		/// 解析伙伴.json，提取 名称 -> (金币获取表达式, 训练加成表达式)
		/// 采用正则提取，避免引入额外Json库
		/// </summary>
		private static Dictionary<string, (string goldExpr, string trainExpr)> ParsePartnerBoosts(string json)
		{
			var result = new Dictionary<string, (string, string)>();

			// 粗略按对象分割：查找每个 { ... }
			foreach (Match m in Regex.Matches(json, "\\{[\\s\\S]*?\\}"))
			{
				string obj = m.Value;
				string name = ExtractString(obj, "名称");
				if (string.IsNullOrEmpty(name)) continue;

				string gold = ExtractString(obj, "加成_百分比_金币获取");
				string train = ExtractString(obj, "加成_百分比_训练加成");

				if (!string.IsNullOrEmpty(gold) && !string.IsNullOrEmpty(train))
				{
					result[name] = (gold, train);
				}
			}

			return result;
		}

		/// <summary>
		/// 从对象片段中提取字符串字段值，例如 "字段名": "值"
		/// </summary>
		private static string ExtractString(string obj, string field)
		{
			string pattern = $"\"{Regex.Escape(field)}\"\\s*:\\s*(\"(?<v>[^\"]*)\"|(?<v>null|[-0-9.]+))";
			var m = Regex.Match(obj, pattern);
			if (!m.Success) return string.Empty;
			string v = m.Groups["v"].Value;
			if (v == "null") return string.Empty;
			return v;
		}

		/// <summary>
		/// 解析翅膀.json，提取 翅膀名称 -> (速度加成, 加速度, 金币加成)
		/// </summary>
		private static Dictionary<string, (string speedExpr, string accelExpr, string goldExpr)> ParseWingBoosts(string json)
		{
			var result = new Dictionary<string, (string, string, string)>();
			foreach (Match m in Regex.Matches(json, "\\{[\\s\\S]*?\\}"))
			{
				string obj = m.Value;
				string name = ExtractString(obj, "翅膀名称");
				if (string.IsNullOrEmpty(name)) continue;

				string speed = ExtractString(obj, "加成_百分比_速度加成");
				string accel = ExtractString(obj, "加成_百分比_加速度");
				string gold = ExtractString(obj, "加成_百分比_金币加成");

				if (!string.IsNullOrEmpty(speed) || !string.IsNullOrEmpty(accel) || !string.IsNullOrEmpty(gold))
				{
					result[name] = (speed, accel, gold);
				}
			}
			return result;
		}

		/// <summary>
		/// 从表达式中提取倍率数字：
		/// - "S_LVL*4.2" -> "4.2"
		/// - "4.2" -> "4.2"
		/// - 其他不规则格式则尽量匹配第一个数字
		/// </summary>
		private static string ExtractMultiplier(string expr)
		{
			if (string.IsNullOrEmpty(expr)) return "";
			string s = expr.Trim();
			// 优先匹配 S_LVL*数字
			var m = Regex.Match(s, @"S_LVL\s*\*\s*(?<num>-?[0-9]+(\.[0-9]+)?)", RegexOptions.IgnoreCase);
			if (m.Success)
			{
				return m.Groups["num"].Value;
			}
			// 纯数字
			if (decimal.TryParse(s, out _))
			{
				return s;
			}
			// 退而求其次，找第一个数字
			var m2 = Regex.Match(s, @"-?[0-9]+(\.[0-9]+)?");
			if (m2.Success)
			{
				return m2.Value;
			}
			return s;
		}
	}
}


