﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;


namespace TheDeconstructor
{
	class DeconGlobalItem : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			var info = item.GetModInfo<DeconItemInfo>(mod);
			if (info.addValueTooltip)
			{
				ItemValue value = new ItemValue().SetFromCopperValue(item.value*item.stack).ToSellValue();
				//tooltips[0].text = $"[i/s1:{item.type}]{tooltips[0].text}";
				//tooltips.Insert(1, new TooltipLine(mod, $"{mod.Name}: ValueTooltip", $"{value}"));
				tooltips[0].text += $"{value.ToTagString()}";
			}
		}
	}

	public class DeconItemInfo : ItemInfo
	{
		public bool addValueTooltip = false;

		public override ItemInfo Clone()
		{
			var clone = new DeconItemInfo();
			clone.addValueTooltip = this.addValueTooltip;
			return clone;
		}
	}
}