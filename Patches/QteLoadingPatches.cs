using System.Reflection;
using SPT.Reflection.Patching;
using EFT.Hideout;
using UnityEngine;
using Random = UnityEngine.Random;

namespace stckytwl.HexagonClickingMinigame.Patches
{
    public class QteLoadBeatMapPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(WorkoutBehaviour).GetMethod("method_12", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static bool PreFix(ref QuickTimeEvent[] quickTimeEvents)
        {
            var newQuickTimeEvents = new QuickTimeEvent[quickTimeEvents.Length];

            for (var i = 0; i < quickTimeEvents.Length; i++)
            {
                var qte = CreateQte(quickTimeEvents[i]);

                newQuickTimeEvents[i] = qte;
            }

            quickTimeEvents = newQuickTimeEvents;

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
}