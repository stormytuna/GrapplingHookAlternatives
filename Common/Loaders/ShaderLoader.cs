using ReLogic.Content;
using Terraria.Graphics.Shaders;

namespace GrapplingHookAlternatives.Common.Loaders;

[Autoload(Side = ModSide.Client)]
public class ShaderLoader : ILoadable
{
	public static MiscShaderData TeleporterShader => GameShaders.Misc[$"{nameof(GrapplingHookAlternatives)}:{nameof(TeleporterShader)}"];
	public static MiscShaderData DeliciousStrawberryShader => GameShaders.Misc[$"{nameof(GrapplingHookAlternatives)}:{nameof(DeliciousStrawberryShader)}"];

	// TODO: Move to raw Effects
	public void Load(Mod mod) {
		Ref<Effect> teleporterShaderRef = new(mod.Assets.Request<Effect>("Assets/Effects/TeleporterPlayer", AssetRequestMode.ImmediateLoad).Value);
		MiscShaderData teleporterShaderData = new(teleporterShaderRef, "Pass1");
		teleporterShaderData.UseColor(Color.Green);
		teleporterShaderData.UseOpacity(0.0f); // Set properly when we actually do our teleport
		teleporterShaderData.UseImage1(mod.Assets.Request<Texture2D>("Assets/Textures/noise01", AssetRequestMode.ImmediateLoad));
		teleporterShaderData.UseImage2(mod.Assets.Request<Texture2D>("Assets/Textures/noise02", AssetRequestMode.ImmediateLoad));
		GameShaders.Misc[$"{nameof(GrapplingHookAlternatives)}:{nameof(TeleporterShader)}"] = teleporterShaderData;

		Ref<Effect> deliciousStrawberryShaderRef = new(mod.Assets.Request<Effect>("Assets/Effects/DeliciousStrawberry", AssetRequestMode.ImmediateLoad).Value);
		MiscShaderData deliciousStrawberryShaderData = new(deliciousStrawberryShaderRef, "Pass1");
		deliciousStrawberryShaderData.UseColor(new Color(119, 252, 250));
		GameShaders.Misc[$"{nameof(GrapplingHookAlternatives)}:{nameof(DeliciousStrawberryShader)}"] = deliciousStrawberryShaderData;
	}

	public void Unload() { }
}
