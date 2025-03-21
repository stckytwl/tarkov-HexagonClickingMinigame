using System.Reflection;
using EFT.Hideout;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using Random = UnityEngine.Random;

namespace stckytwl.HexagonClickingMinigame.Patches;

public class QteLoadBeatMapPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.method_12));
    }

    [PatchPrefix]
    private static bool PreFix(ref QteData qteData)
    {
        var quickTimeEvents = qteData.QuickTimeEvents;
        var newQuickTimeEvents = new QuickTimeEvent[quickTimeEvents.Length];

        for (var i = 0; i < quickTimeEvents.Length; i++)
        {
            var qte = CreateQte(quickTimeEvents[i]);

            newQuickTimeEvents[i] = qte;
        }

        qteData.QuickTimeEvents = newQuickTimeEvents;

        return true;
    }

    private static QuickTimeEvent CreateQte(QuickTimeEvent qte)
    {
        var randomPosition = GetRandomPosition();
        var newQte = new QuickTimeEvent(qte.Type,
            randomPosition,
            qte.Speed,
            qte.StartDelay,
            qte.EndDelay,
            qte.SuccessRange,
            qte.Key);

        return newQte;
    }

    private static Vector2 GetRandomPosition()
    {
        const float margin = 0.1f;

        var x = Random.Range(margin, 1f - margin);
        var y = Random.Range(margin, 1f - margin);

        return new Vector2(x, y);
    }
}