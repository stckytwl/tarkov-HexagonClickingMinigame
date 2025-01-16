using System;
using System.Collections.Generic;
using System.Reflection;
using SPT.Common.Utils;
using BepInEx;
using BepInEx.Configuration;
using SPT.Reflection.Patching;
using stckytwl.HexagonClickingMinigame.Patches;

namespace stckytwl.HexagonClickingMinigame;

[BepInPlugin("com.stckytwl.hexagonclickingminigame", "HexagonClickingMinigame", "2.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static bool IsQteRunning = false;
    public static string Directory { get; private set; }
    public static ConfigEntry<string> BeatmapPath;

    private static readonly List<ModulePatch> Patches = [
            new QteRunningPatch(),
            new QteEndedPatch(),
            new QteEnableCursorPatch(),
            new QteClickablePatch(),
            new QteLoadBeatMapPatch(),
            new QteCreateGameObjectPatch(),
#if DEBUG
            new QteHealthEffectsPatches(),
            new QteNutritionPatches(),
#endif
    ];

    private void Awake()
    {
#if DEBUG
        Directory = @"C:\Games\SPT - 3.10\BepInEx\plugins\HexagonClickingMinigame\";
#else
        Directory = Assembly.GetExecutingAssembly().Location.GetDirectory() + @"\";
#endif

        PluginUtils.Logger = Logger;

        InitializeSettings();

        TogglePatches(true);
    }

    private void OnDestroy()
    {
        TogglePatches(false);
    }

    private void TogglePatches(bool toggle)
    {
        switch (toggle)
        {
            case true:
                foreach (var patch in Patches)
                {
                    patch.Enable();
                }
                Logger.LogInfo("Patches enabled");

                break;
            case false:
                foreach (var patch in Patches)
                {
                    patch.Disable();
                }
                Logger.LogInfo("Patches disabled");

                break;
        }
    }

    private void InitializeSettings()
    {
        BeatmapPath = Config.Bind("",
            "Beatmap file path",
            "tarkin",
            new ConfigDescription("Path to beatmap folder relative to \"BepInEx/Plugins/stckytwl.OSU\""));
    }
}