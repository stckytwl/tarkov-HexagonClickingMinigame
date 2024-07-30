using System.Collections.Generic;

namespace stckytwl.OSU.Models
{
    public class Beatmap
    {
        public readonly string Name;
        public readonly List<HitObject> HitObjects;
        public readonly float CircleSize;

        public Beatmap(string name, List<HitObject> hitObjects, float circleSize)
        {
            Name = name;
            HitObjects = hitObjects;
            CircleSize = circleSize;
        }
    }
}