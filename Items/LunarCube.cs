﻿using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheDeconstructor.Items
{
	internal sealed class LunarCube : Cube
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Lunar Cube");
		}

		public override Color? GetAlpha(Color lightColor) =>
			CubeColor<LunarCube>();

		public override void PostUpdate() =>
			CubeLighting<LunarCube>();

		public override void AddRecipes()
		{
			var recipe = new ModRecipe(mod);

			// Old recipe
			//recipe.AddRecipeGroup("Fragment");
			//recipe.AddIngredient(ItemID.LunarBar);
			//recipe.AddTile(mod.TileType<Tiles.Deconstructor>());

			// New recipe
			recipe.AddIngredient(ItemID.SoulofLight);
			recipe.AddIngredient(ItemID.SoulofNight);
			recipe.AddIngredient(ItemID.FallenStar);
			recipe.AddTile(mod.TileType<Tiles.Deconstructor>());

			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
