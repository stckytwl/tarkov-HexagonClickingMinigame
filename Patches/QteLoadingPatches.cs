using System;
using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Logs;
using EFT.Communications;
using EFT.Hideout;
using stckytwl.OSU.Models;
using UnityEngine;

namespace stckytwl.OSU.Patches
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
            try
            {
                BeatmapLoader.LoadBeatmap();
            }
            #pragma warning disable CS0168
            catch (Exception _)
            #pragma warning restore CS0168
            {
                PluginUtils.Logger.LogError($"Beatmap loading failed! Beatmap file \"{Plugin.Directory + Plugin.BeatmapPath.Value}\" is corrupt");
                NotificationManagerClass.DisplayWarningNotification("Beatmap loading failed!", ENotificationDurationType.Long);
                NotificationManagerClass.DisplayWarningNotification($"Beatmap file \"{Plugin.Directory + Plugin.BeatmapPath.Value}\" is corrupt!", ENotificationDurationType.Long);
                quickTimeEvents = Array.Empty<QuickTimeEvent>();
                return true;
            }

            var beatMapHitObjects = BeatmapLoader.LoadedBeatMap.HitObjects;
            var newQuickTimeEvents = new QuickTimeEvent[beatMapHitObjects.Count];

            for (var i = 0; i < beatMapHitObjects.Count; i++)
            {
                var qte = HitObjectToQuickTimeEvent(beatMapHitObjects[i]);

                newQuickTimeEvents[i] = qte;
            }

            quickTimeEvents = newQuickTimeEvents;

            return true;
        }

        private static QuickTimeEvent HitObjectToQuickTimeEvent(HitObject hitObject)
        {
            const EQteType type = EQteType.ShrinkingCircle;
            var position = hitObject.Position;
            const float speed = 3f;
            var startDelay = (float)hitObject.BeforeWait / 1000 * 0.9f;
            var endDelay = (float)hitObject.BeforeWait / 1000 * 0.1f;
            var successRange = new Vector2(0.5f, 0.1f);
            const KeyCode key = KeyCode.Mouse0;

            QuickTimeEvent qte = new(type, position, speed, startDelay, endDelay, successRange, key);

            PluginUtils.Logger.LogDebug($"{type}, {position}, {speed}, {startDelay}, {endDelay}, {successRange}, {key}");
            return qte;
        }
    }
}