using System;
using System.Collections.Generic;
using Aki.Common.Utils;
using Comfort.Common;
using EFT.Settings.Graphics;
using stckytwl.OSU.Models;
using PeanutButter.INI;
using UnityEngine;

namespace stckytwl.OSU
{
    public static class BeatmapLoader
    {
        private static EftResolution _resolution;
        private static int _lastTime;
        public static Beatmap LoadedBeatMap;

        public static void LoadBeatmap()
        {
            _resolution = GetResolution();

            var beatmapPath = Plugin.Directory + Plugin.BeatmapPath.Value;

            if (!VFS.Exists(beatmapPath) || Plugin.BeatmapPath.Value == "")
            {
                NotificationManagerClass.DisplayWarningNotification($"Beatmap file \"{Plugin.BeatmapPath.Value}\" does not exist!");
                LoadedBeatMap = new Beatmap("", new List<HitObject>(), 0.825f);
                return;
            }

            var rawBeatmapData = VFS.ReadTextFile(beatmapPath);
            var beatmapIni = INIFile.FromString(rawBeatmapData);
            var hitObjectsSection = beatmapIni.GetSection("HitObjects").Keys;
            var difficultySection = beatmapIni.GetSection("Difficulty").Keys;
            var metadataSection = beatmapIni.GetSection("Metadata").Keys;
            var beatmapName = "";

            var hitObjects = new List<HitObject>();

            foreach (var iHitObject in hitObjectsSection)
            {
                if (!ParseHitObject(iHitObject, out var hitObject))
                {
                    continue;
                }
                hitObjects.Add(hitObject);
            }

            foreach (var metadata in metadataSection)
            {
                var name = metadata.Split(new[] { ":" }, StringSplitOptions.None)[1];
                beatmapName = name;
                break;
            }

            var beatmap = new Beatmap(beatmapName, hitObjects, 0.825f);
            LoadedBeatMap = beatmap;
            NotificationManagerClass.DisplayMessageNotification($"Loaded Beatmap {beatmapName}");
        }

        private static bool ParseHitObject(string rawHitObject, out HitObject hitObject)
        {
            var separator = new[] { "," };
            var split = rawHitObject.Split(separator, StringSplitOptions.None);

            var x = int.Parse(split[0]);
            var y = int.Parse(split[1]);
            var time = int.Parse(split[2]);
            var type = (HitTypes)int.Parse(split[3]);
            var objectParams = new List<string>(); // Unused

            _lastTime = time;
            var translatedPosition = TranslatePlayfieldToScreen(x, y);
            var beforeWait = time - _lastTime;

            if ((type & HitTypes.HitCircle) != 0)
            {
                var newHitCircle = new HitCircle(translatedPosition, time, type, beforeWait, objectParams);
                hitObject = newHitCircle;
                return true;
            }

            hitObject = null;
            return false;
        }

        private static Vector2 TranslatePlayfieldToScreen(float x, float y)
        {
            const int osuBaseWidth = 640;
            const int osuBaseHeight = 480;

            var scaleX = _resolution.Width / osuBaseWidth;
            var scaleY = _resolution.Height / osuBaseHeight;

            var translatedX = x * scaleX;
            var translatedY = y * scaleY;

            var normalizedX = translatedX / _resolution.Width;
            var normalizedY = translatedY / _resolution.Height;

            var flippedY = 1 - normalizedY;

            var finalX = normalizedX * 0.8f + 0.1f;
            var finalY = flippedY * 0.8f + 0.1f;

            var finalVector = new Vector2(finalX, finalY);

            return finalVector;
        }

        private static EftResolution GetResolution()
        {
            var settings = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings;
            var displaySettings = (GStruct236)settings.DisplaySettings;
            var resolution = displaySettings.Resolution;

            return resolution;
        }
    }
}