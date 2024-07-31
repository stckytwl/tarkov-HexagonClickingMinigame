using System.Reflection;
using SPT.Reflection.Patching;
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
                PluginUtils.Logger.LogInfo("Player either has a broken arm or is tired. Skipping");
            }
        }
    }
}