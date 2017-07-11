using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;


namespace TheDeconstructor.Items
{
	internal sealed class QueerLunarCube : Cube
	{
		public override string Texture => $"{mod.Name}/Items/LunarCube";

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Queer Lunar Cube");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.rare = 10;
		}

		public override Color? GetAlpha(Color lightColor) =>
			CubeColor<QueerLunarCube>();

		public override void PostUpdate() =>
			CubeLighting<QueerLunarCube>();

		public override void AddRecipes()
		{
			var recipe = new ModRecipe(mod);

			// Old recipe
			//recipe.AddRecipeGroup("Fragment");
			//recipe.AddIngredient(mod.ItemType<LunarCube>(), 1);
			//recipe.AddTile(mod.TileType<Tiles.Deconstructor>());

			// New recipe
			recipe.AddIngredient(mod.ItemType<LunarCube>());
			recipe.AddIngredient(ItemID.SoulofMight);
			recipe.AddIngredient(ItemID.SoulofSight);
			recipe.AddIngredient(ItemID.SoulofFright);
			recipe.AddIngredient(ItemID.FallenStar);
			recipe.AddTile(mod.TileType<Tiles.Deconstructor>());

			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
