using System.Reflection;
using SPT.Reflection.Patching;
using EFT.Hideout;
using UnityEngine;

namespace stckytwl.HexagonClickingMinigame.Patches
{
    public class QteCreateGameObjectPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(QTEController).GetMethod("method_0", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        // ReSharper disable once InconsistentNaming
        private static void PatchPostFix(ref QTEAction __result)
        {
            //PluginUtils.Logger.LogDebug(__result.RectTransform().localScale);
            __result.RectTransform().localScale = new Vector3(1, 1) * BeatmapLoader.LoadedBeatMap.CircleSize;
        }
    }
}