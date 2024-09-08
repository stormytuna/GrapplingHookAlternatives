using ReLogic.Content;
using Terraria.Graphics.Shaders;

namespace GrapplingHookAlternatives.Common.Loaders;

[Autoload(Side = ModSide.Client)]
public class ShaderLoader : ILoadable
{
	public static MiscShaderData TeleporterShader => GameShaders.Misc[$"{nameof(GrapplingHookAlternatives)}:TeleporterShader"];

	public void Load(Mod mod) {
		Ref<Effect> teleporterShaderRef = new(mod.Assets.Request<Effect>("Assets/Effects/TeleporterPlayer", AssetRequestMode.ImmediateLoad).Value);
		MiscShaderData teleporterShaderData = new(teleporterShaderRef, "Pass1");
		teleporterShaderData.UseColor(Color.Green);
		teleporterShaderData.UseOpacity(0.0f); // Set properly when we actually do our teleport
		teleporterShaderData.UseImage1(mod.Assets.Request<Texture2D>("Assets/Textures/noise01", AssetRequestMode.ImmediateLoad));
		teleporterShaderData.UseImage2(mod.Assets.Request<Texture2D>("Assets/Textures/noise02", AssetRequestMode.ImmediateLoad));
		GameShaders.Misc[$"{nameof(GrapplingHookAlternatives)}:TeleporterShader"] = teleporterShaderData;
	}

	public void Unload() { }
}
