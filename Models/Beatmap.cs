using System.Collections.Generic;
using UnityEngine;

namespace stckytwl.HexagonClickingMinigame.Models
{
    public class Beatmap
    {
        public readonly string Name;
        public readonly AudioClip Audio;
        public readonly List<HitObject> HitObjects;
        public readonly float CircleSize;

        public Beatmap(string name, AudioClip audio, List<HitObject> hitObjects, float circleSize)
        {
            Name = name;
            Audio = audio;
            HitObjects = hitObjects;
            CircleSize = circleSize;
        }
    }
}