using System.Reflection;
using SPT.Common.Utils;
using BepInEx;
using BepInEx.Configuration;
using stckytwl.HexagonClickingMinigame.Patches;

namespace stckytwl.HexagonClickingMinigame;

[BepInPlugin("com.stckytwl.hexagonclickingminigame", "HexagonClickingMinigame", "2.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static bool IsQteRunning = false;
    public static string Directory { get; private set; }
    public static ConfigEntry<string> BeatmapPath;

    private void Awake()
    {
        Directory = Assembly.GetExecutingAssembly().Location.GetDirectory() + @"\";
        PluginUtils.Logger = Logger;

        InitializeSettings();

        new QteRunningPatch().Enable();
        new QteEndedPatch().Enable();
        new QteEnableCursorPatch().Enable();
        new QteClickablePatch().Enable();
        new QteLoadBeatMapPatch().Enable();
        new QteCreateGameObjectPatch().Enable();
    }

    private void InitializeSettings()
    {
        BeatmapPath = Config.Bind("",
            "Beatmap file path",
            "tarkin",
            new ConfigDescription("Path to beatmap folder relative to \"BepInEx/Plugins/stckytwl.OSU\""));
    }
}