using System;
using BepInEx;
using DrakiaXYZ.VersionChecker;
using stckytwl.HexagonClickingMinigame.Patches;

namespace stckytwl.HexagonClickingMinigame
{
    [BepInPlugin("com.stckytwl.hexagonclickingminigame", "HexagonClickingMinigame", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {

        private void Awake()
        {
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception($"Invalid EFT Version");
            }

            PluginUtils.Logger = Logger;

            new QteRunningPatch().Enable();
            new QteEnableCursorPatch().Enable();
            new QteClickablePatch().Enable();
            new QteLoadBeatMapPatch().Enable();
        }
    }
}