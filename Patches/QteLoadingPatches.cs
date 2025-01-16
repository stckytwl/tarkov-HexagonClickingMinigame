using System;
using System.Reflection;
using SPT.Reflection.Patching;
using Comfort.Common;
using EFT.Hideout;
using HarmonyLib;
using stckytwl.HexagonClickingMinigame.Models;
using UnityEngine;

namespace stckytwl.HexagonClickingMinigame.Patches;

public class QteLoadBeatMapPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.method_12));
    }

    [PatchPrefix]
    private static bool PreFix(ref QuickTimeEvent[] quickTimeEvents)
    {
        try
        {
            BeatmapLoader.LoadBeatmap();
        }
        catch (Exception ex)
        {
            PluginUtils.Logger.LogError(
                $"Beatmap loading failed! Beatmap file \"{Plugin.Directory + Plugin.BeatmapPath.Value}\" is corrupt");
            PluginUtils.Logger.LogError($"{ex}");

            PluginUtils.DisplayWarningNotification("Beatmap loading failed!");
            PluginUtils.DisplayWarningNotification(
                $"Beatmap file \"{Plugin.Directory + Plugin.BeatmapPath.Value}\" is corrupt!");

            quickTimeEvents = [];

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
            PluginUtils.Logger.LogInfo("LOADING AUDIOOOOOOOOOOOOOOOO");
            var clip = BeatmapLoader.LoadedBeatMap.Audio;
            var source = BetterAudio.AudioSourceGroupType.Nonspatial;
            BeatmapLoader.LoadedAudioSource = Singleton<BetterAudio>.Instance.PlayAtPoint(new Vector3(), clip, 0f, source, 100);
        }

        return true;
    }

    private static QuickTimeEvent HitObjectToQuickTimeEvent(HitObject hitObject)
    {
        const EQteType type = EQteType.ShrinkingCircle;
        var position = hitObject.Position;
        const float speed = 3f;
        // var startDelay = (float)hitObject.BeforeWait / 1000 * 0.9f;
        // var endDelay = (float)hitObject.BeforeWait / 1000 * 0.1f;
        var startDelay = (float)hitObject.BeforeWait;
        var endDelay = (float)hitObject.BeforeWait;

        var successRange = new Vector2(0.5f, 0.1f);
        const KeyCode key = KeyCode.Mouse0;

        QuickTimeEvent qte = new(type, position, speed, startDelay, endDelay, successRange, key);

        //PluginUtils.Logger.LogDebug($"{type}, {position}, {speed}, {startDelay}, {endDelay}, {successRange}, {key}");
        return qte;
    }
}