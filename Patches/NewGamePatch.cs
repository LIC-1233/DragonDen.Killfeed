using System.Reflection;
using Comfort.Common;
using DragonDen.Killfeed.Features;
using EFT;
using SPT.Reflection.Patching;

namespace DragonDen.Killfeed.Patches;

internal class NewGamePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));

    [PatchPrefix]
    private static void PatchPrefix()
    {
        var gw = Singleton<GameWorld>.Instance;
        if (gw == null) return;
        gw.gameObject.AddComponent<KillfeedController>();
    }
}