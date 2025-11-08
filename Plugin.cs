using BepInEx;
using BepInEx.Logging;
using DragonDen.Killfeed.Patches;
using DragonDen.Killfeed.Utilities;

namespace DragonDen.Killfeed;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.SPT.custom", "4.0")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; set; }
    
    public void Awake()
    {
        Logger ??= BepInEx.Logging.Logger.CreateLogSource("DragonDen.Killfeed");
        Settings.Init(Config);
        PatchManager.EnablePatches();
    }
}