using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Aki.Common.Utils;
using Comfort.Common;
using EFT.Settings.Graphics;
using PeanutButter.INI;
using stckytwl.OSU.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace stckytwl.OSU
{
    public static class BeatmapLoader
    {
        private static EftResolution _resolution;
        private static int _lastTime;
        public static Beatmap LoadedBeatMap;
        public static BetterSource LoadedAudioSource;

        public static void LoadBeatmap()
        {
            _resolution = GetResolution();

            var beatmapPath = Plugin.Directory + Plugin.BeatmapPath.Value;

            if (!VerifyBeatmapFolder(beatmapPath, out var osuFile, out var audioClip))
            {
                LoadedBeatMap = new Beatmap("", null, new List<HitObject>(), 0.825f);
                return;
            }

            var rawBeatmapData = VFS.ReadTextFile(osuFile);
            var beatmapIni = INIFile.FromString(rawBeatmapData);
            var hitObjectsSection = beatmapIni.GetSection("HitObjects").Keys;
            // var difficultySection = beatmapIni.GetSection("Difficulty").Keys; // Unused
            var metadataSection = beatmapIni.GetSection("Metadata").Keys;
            var beatmapName = "";

            var hitObjects = new List<HitObject>();

            foreach (var iHitObject in hitObjectsSection)
            {
                if (!ParseHitObject(iHitObject, out var hitObject)) continue;
                hitObjects.Add(hitObject);
            }

            foreach (var metadata in metadataSection)
            {
                var name = metadata.Split(new[]
                    {
                        ":"
                    },
                    StringSplitOptions.None)[1];
                beatmapName = name;
                break;
            }

            var beatmap = new Beatmap(beatmapName, audioClip, hitObjects, 0.825f);
            LoadedBeatMap = beatmap;
            PluginUtils.DisplayMessageNotification($"Loaded Beatmap {beatmapName}");
        }

        private static bool VerifyBeatmapFolder(string folderPath, out string osuFile, out AudioClip audioFile)
        {
            osuFile = null;
            audioFile = null;

            if (!Directory.Exists(folderPath))
            {
                PluginUtils.Logger.LogWarning($"Beatmap folder \"{Plugin.BeatmapPath.Value}\" does not exist!");
                PluginUtils.DisplayWarningNotification($"Beatmap folder \"{Plugin.BeatmapPath.Value}\" does not exist!");
                return false;
            }

            var dir = Directory.EnumerateFiles(folderPath);

            PluginUtils.Logger.LogInfo($"Searching folder \"{folderPath}\" for beatmap files...");

            foreach (var file in dir) // my bad.
            {
                switch (Path.GetExtension(file))
                {
                    case ".osu":
                        if (osuFile is null)
                        {
                            osuFile = file;
                            PluginUtils.Logger.LogInfo($"Using \"{osuFile}\" as osu file.");
                        }

                        break;
                    case ".wav":
                        if (audioFile is null)
                        {
                            PluginUtils.Logger.LogInfo($"Attempting to load \"{file}\" as audio file.");
                            var clip = LoadAudioFile(file);
                            if (clip.Result is null)
                            {
                                PluginUtils.Logger.LogWarning($"Audio file \"{file}\" is unsupported or corrupt!");
                                continue;
                            }

                            audioFile = clip.Result;
                            PluginUtils.Logger.LogInfo($"Using \"{audioFile}\" as audio file.");
                        }

                        break;
                }
            }

            return osuFile is not null && audioFile is not null;
        }

        private static bool ParseHitObject(string rawHitObject, out HitObject hitObject)
        {
            var separator = new[]
            {
                ","
            };
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

        private static async Task<AudioClip> LoadAudioFile(string audioPath)
        {
            var extension = Path.GetExtension(audioPath);
            AudioType audioType;
            switch (extension)
            {
                case ".wav":
                    audioType = AudioType.WAV;
                    break;
                case ".ogg":
                    audioType = AudioType.OGGVORBIS;
                    break;
                case ".mp3":
                    audioType = AudioType.MPEG;
                    break;
                default:
                    PluginUtils.Logger.LogWarning(
                        $"\"{Path.GetFileName(audioPath)}\" is not a valid audio file!");
                    return null;
            }

            var uwr = UnityWebRequestMultimedia.GetAudioClip(audioPath, audioType);
            var sendWeb = uwr.SendWebRequest();

            while (!sendWeb.isDone) await Task.Yield();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                PluginUtils.Logger.LogError(
                    $"Failed To Fetch Audio Clip \"{Path.GetFileNameWithoutExtension(audioPath)}\"");
                return null;
            }

            var audioClip = DownloadHandlerAudioClip.GetContent(uwr);
            audioClip.name = Path.GetFileNameWithoutExtension(audioPath);
            return audioClip;
        }
    }
}