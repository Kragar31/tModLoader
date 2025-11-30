using ExampleMod.Content.Tiles;
using Humanizer;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ExampleMod.Common.Systems
{
	// This ModSystem will demonstrate how to add a task to the list of Hardmode generation tasks
	public class ExampleHardmodeGenSystem : ModSystem
	{
		public static LocalizedText GeneratedHardmodeBlocksMessage { get; private set; }

		// Set the GenerateHardmodeBlocksMessage to its text from the localization
		public override void SetStaticDefaults() {
			GeneratedHardmodeBlocksMessage = Mod.GetLocalization($"WorldGen.{nameof(GeneratedHardmodeBlocksMessage)}");
		}

		// We use ModifyHardmodeTasks and tasks.Add to add in a new task after all other Hardmode tasks have been added
		public override void ModifyHardmodeTasks(List<GenPass> tasks) {
			tasks.Add(new ExampleHardmodeGenPass("Generating example blocks.", 237.4298f));
		}
	}

	// This GenPass handles the actual pass and runs the code
	public class ExampleHardmodeGenPass : GenPass
	{
		public ExampleHardmodeGenPass(string name, float loadWeight) : base(name, loadWeight) {
		}

		// Applies the pass when it is added to the task list
		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
			// Sends a message to the server when this pass is called to let the player know this task is being executed
			if (Main.netMode == NetmodeID.SinglePlayer) {
				Main.NewText(ExampleHardmodeGenSystem.GeneratedHardmodeBlocksMessage.Value, 50, 255, 130);
			}
			else if (Main.netMode == NetmodeID.Server) {
				ChatHelper.BroadcastChatMessage(ExampleHardmodeGenSystem.GeneratedHardmodeBlocksMessage.ToNetworkText(), new Color(50, 255, 130));
			}

			// This for loop causes the generation code to be executed multiple times, scaling with the size of the world
			// "6E-05" is "scientific notation". It simply means 0.00006 but in some ways is easier to read.
			for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 6E-05); k++) {
				// The inside of this for loop corresponds to one execution of the generation code
				// First, we randomly choose a coordinate in the world anywhere on the surface
				int x = WorldGen.genRand.Next(0, Main.maxTilesX);

				// We use Main.worldSurface because that information is saved after world generation and can be referenced
				// during a Hardmode generation task.
				int y = WorldGen.genRand.Next((int)Main.worldSurface - 50, (int)Main.worldSurface);

				// Then, we call WorldGen.OreRunner with random "strength" and random "steps", as well as the Tile we wish to place.
				// Feel free to experiment with strength and step to see the shape they generate
				// OreRunner is generally not meant to be used during Hardmode generation, as it overwrites all blocks it affects
				WorldGen.OreRunner(x, y, WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(2, 6), (ushort)ModContent.TileType<ExampleBlock>());
			}

			// As another example, we could call the BlessWorldWithExampleOre method from ExampleOreSystem to cause
			// ExampleOre to generate when the world enters Hardmode
			ModContent.GetInstance<ExampleOreSystem>().BlessWorldWithExampleOre();
		}
	}
}
