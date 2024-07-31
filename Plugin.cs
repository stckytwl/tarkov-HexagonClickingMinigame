using System;
using System.Reflection;
using Aki.Common.Utils;
using BepInEx;
using stckytwl.CircleClickingGame.Patches;
using DrakiaXYZ.VersionChecker;

namespace stckytwl.CircleClickingGame
{
    [BepInPlugin("com.stckytwl.circleclickinggame", "CircleClickingGame", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static string Directory { get; private set; }

        private void Awake()
        {
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception($"Invalid EFT Version");
            }

            Directory = Assembly.GetExecutingAssembly().Location.GetDirectory() + @"\";
            PluginUtils.Logger = Logger;

            new QteRunningPatch().Enable();
            new QteEnableCursorPatch().Enable();
            new QteClickablePatch().Enable();
            new QteLoadBeatMapPatch().Enable();
        }
    }
}