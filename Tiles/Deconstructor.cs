using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Net;
using Terraria.ObjectData;
using TheDeconstructor.Items;

namespace TheDeconstructor.Tiles
{
	internal sealed class DeconstructorTE : ModTileEntity
	{
		public bool isOpened { get; internal set; } = false;
		public int? openedPlayer { get; internal set; }
		public float frame = 0;
		public bool updated = false;

		public Vector2[] playerDistances = new Vector2[Main.maxPlayers];

		public void UpdateIsOpened(bool? state = null)
		{
			isOpened = state ?? !isOpened;
			openedPlayer = isOpened
				? Main.myPlayer as int?
				: null;

			updated = true;

			//var inst = TheDeconstructor.instance.deconGUI;

			//if (Main.netMode == NetmodeID.MultiplayerClient
			//	&& inst.currentTEPosition.HasValue)
			//{
			//	var packet = mod.GetPacket();
			//	packet.Write((byte)ModPacketMessage.UpdateIsOpened);
			//	packet.Write(isOpened);
			//	packet.WritePackedVector2(new Vector2(inst.currentTEPosition.Value.X, inst.currentTEPosition.Value.Y));
			//	packet.Write(Main.myPlayer);
			//	packet.Send();
			//}

		}

		public override bool ValidTile(int i, int j)
		{
			var tile = Main.tile[i, j];
			return
				tile.active()
				&& tile.type == mod.TileType<Deconstructor>()
				&& tile.IsTopLeftFrame();
		}

		public override void Update()
		{
			foreach (Player player in Main.player)
			{
				if (!player.active) continue;
				playerDistances[player.whoAmI] 
					= new Vector2(Position.X + 2, Position.Y + 2) * 16f - player.position;
			}

			if (updated)
			{
				NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
				updated = false;
			}
		}

		public override void NetSend(BinaryWriter writer, bool lightSend)
		{
			writer.Write(isOpened);
			writer.Write(openedPlayer ?? -1);
			writer.Write(frame);
			foreach (Vector2 distance in playerDistances)
			{
				writer.WritePackedVector2(distance);
			}
		}

		public override void NetReceive(BinaryReader reader, bool lightReceive)
		{
			isOpened = reader.ReadBoolean();
			int player = reader.ReadInt32();
			openedPlayer = player != 1 ? player as int? : null;
			frame = reader.ReadSingle();
			for (int i = 0; i < playerDistances.Length; i++)
			{
				playerDistances[i] = reader.ReadPackedVector2();
			}
		}

		//public override TagCompound Save()
		//{
		//	return new TagCompound()
		//	{
		//		["isOpened"] = isOpened,
		//		["openedPlayer"] = openedPlayer ?? -1,
		//		["frame"] = frame
		//	};
		//}

		//public override void Load(TagCompound tag)
		//{
		//	isOpened = tag.GetBool("isOpened");
		//	int player = tag.GetInt("openedPlayer");
		//	openedPlayer = player != 1 ? player as int? : null;
		//	frame = tag.GetShort("frame");
		//}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
		{
			// Subtract the origin
			//i -= 2;
			//j -= 2;
			// Singleplayer
			if (Main.netMode != NetmodeID.MultiplayerClient)
				return Place(i, j);
			// Multiplayer
			NetMessage.SendTileSquare(Main.myPlayer, i, j, 5, TileChangeType.None);
			NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
			return -1;
		}
	}

	internal sealed class Deconstructor : ModTile
	{
		private const int s = 16;
		private const int p = 2;

		public override void SetDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateWidth = s;
			TileObjectData.newTile.CoordinatePadding = p;
			TileObjectData.newTile.CoordinateHeights = new int[] { s, s, s };
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity<DeconstructorTE>().Hook_AfterPlacement, -1, 0, true);
			TileObjectData.newTile.Origin = new Point16(2, 2);
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("The Deconstructor");
			AddMapEntry(new Color(50, 50, 50), name);

			disableSmartCursor = true;
		}

		public override void RightClick(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			if (tile.type == Type)
			{
				var instance = TheDeconstructor.instance.deconGUI;
				var TEPos = tile.GetTopLeftFrame(i, j, s, p);
				//var TE = (TileEntity.ByPosition[TEPos] as DeconstructorTE);
				instance.currentTEPosition = TEPos;
				int id = mod.GetTileEntity<DeconstructorTE>().Find(TEPos.X, TEPos.Y);
				if (id != -1)
				{
					DeconstructorTE TE = (DeconstructorTE) TileEntity.ByID[id];
					if (TE.isOpened
						&& TE.openedPlayer.HasValue
						&& TE.openedPlayer != Main.myPlayer)
						return;

					TheDeconstructor.instance.TryToggleGUI();
					TE.UpdateIsOpened();
				}

			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Color useColor = Color.White;
			Tile tile = Main.tile[i, j];

			if (tile.type == Type)
			{
				var inst = TheDeconstructor.instance;

				if (inst.deconGUI.currentTEPosition.HasValue
					&& inst.deconGUI.currentTEPosition.Value == tile.GetTopLeftFrame(i, j, s, p)
					&& inst.deconGUI.visible
					&& !inst.deconGUI.cubeItemPanel.item.IsAir
					&& inst.deconGUI.cubeItemPanel.item.modItem is QueerLunarCube)
				{
					useColor = Main.DiscoColor;
				}
			}

			var sine = (float)Math.Sin(Main.essScale * 0.50f);
			r = 0.05f + 0.35f * sine * useColor.R * 0.01f;
			g = 0.05f + 0.35f * sine * useColor.G * 0.01f;
			b = 0.05f + 0.35f * sine * useColor.B * 0.01f;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			TheDeconstructor.instance.TryToggleGUI(false);
			Item.NewItem(i * 16, j * 16, 20, 28, mod.ItemType<Items.Deconstructor>());
			mod.GetTileEntity<DeconstructorTE>().Kill(i, j);
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			var tile = Main.tile[i, j];
			// Only draw from top left tile
			if (tile.type == Type
				&& tile.IsTopLeftFrame())
			{
				var inst = TheDeconstructor.instance;
				var top = tile.GetTopLeftFrame(i, j, s, p);

				if (inst.deconGUI.visible
					&& !inst.deconGUI.cubeItemPanel.item.IsAir
					&& inst.deconGUI.currentTEPosition.HasValue
					&& inst.deconGUI.currentTEPosition.Value == top)
				{
					int id = mod.GetTileEntity<DeconstructorTE>().Find(top.X, top.Y);
					if (id != 1)
					{
						DeconstructorTE TE = (DeconstructorTE) TileEntity.ByID[id];

						var cube = inst.deconGUI.cubeItemPanel.item.modItem;
						Color useColor =
							 cube is QueerLunarCube
								? (cube as Cube).CubeColor<QueerLunarCube>()
								: (cube as Cube).CubeColor<LunarCube>();

						Vector2 zero = Main.drawToScreen
							? Vector2.Zero
							: new Vector2(Main.offScreenRange, Main.offScreenRange);

						Texture2D animTexture = mod.GetTexture("Items/LunarCube");
						const int frameWidth = 20;
						const int frameHeight = 28;
						Vector2 offset = new Vector2(36f, 8f); // offset 2.5 tiles horizontal, 0.5 tile vertical
						Vector2 position = new Vector2(i, j) * 16f - Main.screenPosition + offset;
						Vector2 origin = new Vector2(frameHeight, frameWidth) * 0.5f;
						// tiles draw every 5 ticks, so we can safely increment here
						TE.frame = (TE.frame + 0.75f) % 8;
						spriteBatch.Draw(animTexture, position + zero,
							new Rectangle(0, frameHeight * (int)TE.frame, frameWidth, frameHeight), useColor, 0f, origin, 1f,
							SpriteEffects.None, 0f);
					}
				}
			}
		}
	}
}