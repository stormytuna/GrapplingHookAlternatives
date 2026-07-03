using ReLogic.Content;

namespace GrapplingHookAlternatives.Common.Loaders;

[Autoload(Side = ModSide.Client)]
public class Assets : ILoadable
{
	public static Asset<Texture2D> Noise01;
	public static Asset<Texture2D> Noise02;

	public static Asset<Effect> TeleporterShader;
	public static Asset<Effect> DeliciousStrawberryShader;

	public void Load(Mod mod) {
		Noise01 = mod.Assets.Request<Texture2D>("Assets/Textures/Noise01");
		Noise02 = mod.Assets.Request<Texture2D>("Assets/Textures/Noise02");

		TeleporterShader = mod.Assets.Request<Effect>("Assets/Effects/TeleporterPlayer");

		DeliciousStrawberryShader = mod.Assets.Request<Effect>("Assets/Effects/DeliciousStrawberry");
	}

	public void Unload() { }
}
