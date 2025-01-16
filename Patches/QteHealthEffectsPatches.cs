using System.Reflection;
using EFT.Hideout;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace stckytwl.HexagonClickingMinigame.Patches;

public class QteHealthEffectsPatches : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(HealthControllerClass), nameof(HealthControllerClass.CreateMusclePainEffect));
    }

    [PatchPostfix]
    public static void PatchPostFix(ref float duration)
    {
        duration = 0f;
    }
}

public class QteNutritionPatches : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.method_13));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}