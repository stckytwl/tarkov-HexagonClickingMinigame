using System;
using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
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
            catch (Exception)
            {
                PluginUtils.Logger.LogError($"Beatmap loading failed! Beatmap file \"{Plugin.Directory + Plugin.BeatmapPath.Value}\" is corrupt");
                PluginUtils.DisplayWarningNotification("Beatmap loading failed!");
                PluginUtils.DisplayWarningNotification($"Beatmap file \"{Plugin.Directory + Plugin.BeatmapPath.Value}\" is corrupt!");
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

            if (BeatmapLoader.LoadedBeatMap.Audio is not null)
            {
                BeatmapLoader.LoadedAudioSource = Singleton<BetterAudio>.Instance.PlayAtPoint(new Vector3(),
                    BeatmapLoader.LoadedBeatMap.Audio,
                    0f,
                    BetterAudio.AudioSourceGroupType.Nonspatial,
                    100);
            }

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