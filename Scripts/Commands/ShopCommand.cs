using UnityEngine;

namespace MiGame.Commands
{
	/// <summary>
	/// 商城指令的操作类型
	/// </summary>
	public enum ShopOperationType
	{
		模拟迷你币购买
	}

	/// <summary>
	/// 模拟商城购买指令
	/// 格式：shop {"操作类型":"模拟迷你币购买","商品ID":"12345","数量":1}
	/// </summary>
	[Command("shop", "模拟商城购买指令")]
	public class ShopCommand : AbstractCommand
	{
		[CommandParamDesc("操作类型（目前仅支持：模拟迷你币购买）")]
		public ShopOperationType 操作类型;

		[CommandParamDesc("商品ID（字符串）")]
		public string 商品ID;

		[CommandParamDesc("购买数量（默认1）")]
		public int 数量 = 1;

		public override void Execute()
		{
			// 基础校验
			if (string.IsNullOrEmpty(商品ID))
			{
				Debug.LogError("商城指令：商品ID不能为空");
				return;
			}
			if (数量 <= 0)
			{
				Debug.LogError("商城指令：数量必须大于0");
				return;
			}

			switch (操作类型)
			{
				case ShopOperationType.模拟迷你币购买:
					SimulateMiniCoinPurchase();
					break;
				default:
					Debug.LogError($"商城指令：不支持的操作类型 {操作类型}");
					break;
			}
		}

		/// <summary>
		/// 模拟迷你币购买流程（仅日志与最小校验，不接入真实支付）
		/// </summary>
		private void SimulateMiniCoinPurchase()
		{
			// 这里仅进行最小实现：打印日志，表示已模拟购买
			Debug.Log($"商城指令：模拟迷你币购买成功，商品ID={商品ID}，数量={数量}");
		}
	}
}
