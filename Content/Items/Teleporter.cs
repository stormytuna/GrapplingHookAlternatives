using System.Collections.Generic;
using System.IO;
using GrapplingHookAlternatives.Common.Abstracts;
using GrapplingHookAlternatives.Common.Loaders;
using GrapplingHookAlternatives.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace GrapplingHookAlternatives.Content.Items;

public class Teleporter : MovementEquipment
{
    public override int CooldownTime => 0 * 60;

    public override void SetDefaults() {
        Item.width = 14;
        Item.height = 34;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.buyPrice(gold: 1);

        Item.shoot = ModContent.ProjectileType<FakeHookProjectile>();
    }

    public override void OnGrapple(Player player) {
        // TODO: Visualise and finalise this!
        int teleportBoxWidth = 16;
        int teleportBoxHeight = 12;
        Point teleportBoxPosition = player.Center.ToTileCoordinates() + new Point(25 * player.direction, 0) - new Point(teleportBoxWidth / 2, teleportBoxHeight / 2);
        //teleportBoxPosition = teleportBoxPosition.Clamp(0, Main.maxTilesX, 0, Main.maxTilesY);

        // width -1 and height -1 so we don't query tiles outside of our box
        List<Point> validPositions = new();
        List<Point> goodPositions = new();
        for (int i = 0; i < teleportBoxWidth - 1; i++) {
            for (int j = 0; j < teleportBoxHeight - 2; j++) {
                Point tilePosition = teleportBoxPosition + new Point(i, j);

                bool emptyAir = !Collision.SolidTilesVersatile(tilePosition.X, tilePosition.X + 1, tilePosition.Y, tilePosition.Y + 2);
                bool solidFloor = WorldGen.SolidTile2(tilePosition.X, tilePosition.Y + 3) && WorldGen.SolidTile2(tilePosition.X + 1, tilePosition.Y + 3);
                if (emptyAir && solidFloor) {
                    goodPositions.Add(tilePosition);
                } else if (emptyAir && !solidFloor) {
                    validPositions.Add(tilePosition);
                }
            }
        }

        // TODO: This glitches and places camera in hell for some reason, figure out why!
        if (goodPositions.Count == 0 && validPositions.Count == 0) {
            Point randomPoint = new(Main.rand.Next(teleportBoxPosition.X, teleportBoxPosition.X + teleportBoxWidth), Main.rand.Next(teleportBoxPosition.Y, teleportBoxPosition.X + teleportBoxHeight));
            Vector2 randomPosition = randomPoint.ToVector2() * 16f;
            player.Teleport(randomPosition);
            // One of rod of discord's death messages
            player.KillMe(PlayerDeathReason.ByOther(Main.rand.Next(13, 15)), 1, 0);
            return;
        }

        Point chosenPosition = goodPositions.Count > 0 ? Main.rand.Next(goodPositions) : Main.rand.Next(validPositions);
        Vector2 teleportPosition = chosenPosition.ToVector2() * 16f;
        player.GetModPlayer<TeleporterPlayer>().BeginTeleporting(teleportPosition);
    }
}

public class TeleporterPlayer : ModPlayer
{
    private const int TeleportTimerMax = 20;

    private static RenderTarget2D teleporterPlayerTarget;
    private static bool renderPlayerHack;

    private static readonly SoundStyle teleportSound = new($"{nameof(GrapplingHookAlternatives)}/Assets/Sounds/teleport") {
        PitchVariance = 0.1f,
        MaxInstances = 0
    };

    private Vector2 teleportPosition;
    private int timer;

    // TODO: Kill the player when they're silly :PPP
    public void BeginTeleporting(Vector2 teleportPosition) {
        if (timer <= 0) {
            timer = TeleportTimerMax;
            this.teleportPosition = teleportPosition;
            SoundEngine.PlaySound(teleportSound, Player.Center);
        }
    }

    public override void UpdateEquips() {
        if (timer == TeleportTimerMax / 2) {
            Vector2 oldPosition = Player.position;
            Player.Teleport(teleportPosition, -1);
            teleportPosition = oldPosition; // Hack to draw the player where we were, TODO: Make this not hacky? i guess?

            for (int i = 0; i < 4; i++) {
                Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<TeleporterDust>());
            }
        }

        if (timer > 0) {
            Lighting.AddLight(Player.Center, Color.LimeGreen.ToVector3() * 0.5f);
            Lighting.AddLight(teleportPosition, Color.LimeGreen.ToVector3() * 0.5f);

            if (Main.rand.NextBool()) {
                Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<TeleporterDust>());
            }
        }

        timer--;
    }

    public override void Load() {
        Main.QueueMainThreadAction(() => { teleporterPlayerTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight); });

        // TODO: Move out into own thing?
        On_Main.CheckMonoliths += static orig => {
            orig();

            if (Main.gameMenu) {
                return;
            }

            for (int i = 0; i < Main.maxPlayers; i++) {
                if (Main.player[i].active) {
                    break;
                }

                if (i == Main.maxPlayers - 1) {
                    return;
                }
            }

            RenderTargetBinding[] oldTargets = Main.graphics.GraphicsDevice.GetRenderTargets();

            Main.graphics.GraphicsDevice.SetRenderTarget(teleporterPlayerTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            for (int i = 0; i < Main.maxPlayers; i++) {
                Player player = Main.player[i];
                if (!player.active) {
                    continue;
                }

                renderPlayerHack = true;
                Main.PlayerRenderer.DrawPlayer(Main.Camera, player, player.position, player.fullRotation, player.fullRotationOrigin);
                renderPlayerHack = false;
            }

            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
        };

        // TODO: Move into an IL edit to prevent breaking other detours

        On_LegacyPlayerRenderer.DrawPlayerInternal += static (orig, self, camera, player, position, rotation, origin, shadow, alpha, scale, only) => {
            int timer = player.GetModPlayer<TeleporterPlayer>().timer;
            if (timer <= 0 || renderPlayerHack) {
                orig(self, camera, player, position, rotation, origin, shadow, alpha, scale, only);
                return;
            }

            int fadeInOutFrames = 5;
            float opacity = 1f;
            if (timer >= TeleportTimerMax - fadeInOutFrames) {
                opacity = MathHelper.Lerp(0, 1f, (TeleportTimerMax - timer) / (float)fadeInOutFrames);
            } else if (timer <= fadeInOutFrames) {
                opacity = MathHelper.Lerp(0, 1f, timer / (float)fadeInOutFrames);
            }

            ShaderLoader.TeleporterShader.UseOpacity(opacity);
            ShaderLoader.TeleporterShader.Apply();
            Main.spriteBatch.Draw(teleporterPlayerTarget, Vector2.Zero, null, Color.White);
            Main.spriteBatch.Draw(teleporterPlayerTarget, player.GetModPlayer<TeleporterPlayer>().teleportPosition - player.position + new Vector2(0f, 8f * player.gravDir), null, Color.White);
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        };
    }

    public override void Unload() {
        Main.QueueMainThreadAction(() => { teleporterPlayerTarget.Dispose(); });
    }

    public class DebugDrawCommand : ModCommand
    {
        public override string Command => "draw";
        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args) {
            using (MemoryStream stream = new()) {
                teleporterPlayerTarget.SaveAsPng(stream, teleporterPlayerTarget.Width, teleporterPlayerTarget.Height);
                File.WriteAllBytes("/tmp/wowlookarendertarget.png", stream.ToArray());
            }
        }
    }
}

public class TeleporterVisualiser : PlayerDrawLayer
{
    public static bool Display;

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => Display;

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        int teleportBoxWidth = 16;
        int teleportBoxHeight = 12;
        Point teleportBoxPosition = Main.LocalPlayer.Center.ToTileCoordinates() + new Point(25 * Main.LocalPlayer.direction, 0) - new Point(teleportBoxWidth / 2, teleportBoxHeight / 2);
        //teleportBoxPosition = teleportBoxPosition.Clamp(0, Main.maxTilesX, 0, Main.maxTilesY);

        for (int i = 0; i < teleportBoxWidth; i++) {
            for (int j = 0; j < teleportBoxHeight; j++) {
                Point tilePosition = teleportBoxPosition + new Point(i, j);
                Vector2 tileScreenTopLeft = tilePosition.ToVector2() * 16f - Main.screenPosition;

                bool emptyAir = !Collision.SolidTilesVersatile(tilePosition.X, tilePosition.X + 1, tilePosition.Y, tilePosition.Y + 2);
                bool solidFloor = WorldGen.SolidTile2(tilePosition.X, tilePosition.Y + 3) && WorldGen.SolidTile2(tilePosition.X + 1, tilePosition.Y + 3);
                if (emptyAir && solidFloor) {
                    Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)tileScreenTopLeft.X, (int)tileScreenTopLeft.Y, 16, 16), Color.Green * 0.3f);
                } else if (emptyAir && !solidFloor) {
                    Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)tileScreenTopLeft.X, (int)tileScreenTopLeft.Y, 16, 16), Color.Orange * 0.3f);
                } else {
                    Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)tileScreenTopLeft.X, (int)tileScreenTopLeft.Y, 16, 16), Color.Red * 0.3f);
                }
            }
        }
    }

    public class ToggleCommand : ModCommand
    {
        public override string Command => "tpv";
        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args) {
            Display = !Display;
        }
    }
}
