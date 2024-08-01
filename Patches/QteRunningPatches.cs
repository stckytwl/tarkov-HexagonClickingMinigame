using System.Reflection;
using SPT.Reflection.Patching;
using EFT;
using EFT.Hideout;
using HarmonyLib;

namespace stckytwl.HexagonClickingMinigame.Patches
{
    public class QteRunningPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.StartQte));
        }

        [PatchPostfix]
        // ReSharper disable once InconsistentNaming
        private static void PatchPostfix(WorkoutBehaviour __instance)
        {
            var isArmBroken = __instance.Boolean_0;
            var isTired = __instance.Boolean_1;

            if (isArmBroken || !isTired)
            {
                Logger.LogInfo("Player either has a broken arm or is tired. Skipping");
                return;
            }

            Plugin.IsQteRunning = true;
        }
    }

    public class QteEndedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(HideoutPlayerOwner).GetMethod("ExitQte", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        // ReSharper disable once InconsistentNaming
        private static void PatchPostfix(HideoutPlayerOwner __instance)
        {
            Plugin.IsQteRunning = false;
            BeatmapLoader.LoadedAudioSource?.Release();
            BeatmapLoader.LoadedAudioSource = null;
        }
    }
}