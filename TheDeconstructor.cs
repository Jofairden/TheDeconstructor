using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.Net;
using Terraria.UI;
using TheDeconstructor.Tiles;
using TheDeconstructor.UI;

namespace TheDeconstructor
{
	public class TheDeconstructor : Mod
	{
		internal UserInterface deconUI;
		internal DeconstructorGUI deconGUI;
		internal static TheDeconstructor instance;
		internal static Texture2D DogeTexture;

		public TheDeconstructor()
		{
			Properties = new ModProperties
			{
				Autoload = true,
				AutoloadSounds = true
			};
		}

		public override void Load()
		{
			instance = this;
			if (Main.dedServ) return;

			DogeTexture = GetTexture("EmptyDoge");
			DogeTexture.MultiplyColorsByAlpha();

			deconUI = new UserInterface();
			deconGUI = new DeconstructorGUI();
			deconGUI.Activate();
			deconUI.SetState(deconGUI);
		}


		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int insertLayer = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (insertLayer != -1)
			{
				layers.Insert(insertLayer, new LegacyGameInterfaceLayer($"{instance.Name}: UI",
					delegate
					{
						if (deconGUI.visible)
						{
							deconUI.Update(Main._drawInterfaceGameTime);
							deconGUI.Draw(Main.spriteBatch);
						}
						return true;
					},
					InterfaceScaleType.UI));
			}

			insertLayer = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Interact Item Icon"));
			layers[insertLayer].Active = !(insertLayer != -1 && deconGUI.visible && deconGUI.IsMouseHovering);
		}

		/// <summary>
		/// Try toggling our UI
		/// </summary>
		public void TryToggleGUI(bool state)
		{
			bool visible = state;

			SoundHelper.PlaySound(
				visible
				? SoundHelper.SoundType.OpenUI
				: SoundHelper.SoundType.CloseUI);

			deconGUI.visible = visible;
			deconGUI.ToggleUI(visible);
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			ModPacketType type = (ModPacketType)reader.ReadInt32();

			if (type == ModPacketType.RequestOwnership)
			{
				int ID = reader.ReadInt32();
				if (TileEntity.ByID.ContainsKey(ID))
				{
					DeconstructorTE TE = (DeconstructorTE)TileEntity.ByID[ID];

					if (TE.playerWhoAmI == -1)
					{
						TE.playerWhoAmI = reader.ReadInt32();

						var packet = GetPacket();
						packet.Write((int)ModPacketType.GrantOwnership);
						packet.Send(TE.playerWhoAmI);
					}
				}
			}
			else if (type == ModPacketType.RelinquishOwnership)
			{
				int ID = reader.ReadInt32();
				if (TileEntity.ByID.ContainsKey(ID))
				{
					DeconstructorTE TE = (DeconstructorTE) TileEntity.ByID[ID];
					int owner = reader.ReadInt32();
					if (TE.playerWhoAmI == owner)
					{
						TE.playerWhoAmI = -1;

						var packet = GetPacket();
						packet.Write((int)ModPacketType.TakeOwnership);
						packet.Send(owner);
					}
				}
			}
			else if (type == ModPacketType.GrantOwnership)
			{
				TryToggleGUI(true);
			}
			else if (type == ModPacketType.TakeOwnership)
			{
				TryToggleGUI(false);
			}
		}
	}


	public enum ModPacketType
	{
		RequestOwnership,
		RelinquishOwnership,
		GrantOwnership,
		TakeOwnership
	}
}
