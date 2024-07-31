using System;
using System.Collections.Generic;
using UnityEngine;

namespace stckytwl.OSU.Models
{
    public abstract class HitObject
    {
        public Vector2 Position;
        public int Time;
        public HitTypes Type;
        public int BeforeWait;
        public List<string> ObjectParams;

        public HitObject(Vector2 position, int time, HitTypes type, int beforeWait, List<string> objectParams)
        {
            Position = position;
            Time = time;
            Type = type;
            BeforeWait = beforeWait;
            ObjectParams = objectParams;
        }
    }

    public class HitCircle : HitObject
    {
        public HitCircle(Vector2 position, int time, HitTypes type, int beforeWait, List<string> objectParams) :
            base(position, time, type, beforeWait, objectParams)
        {
        }
    }

    public class Slider : HitObject
    {
        public Slider(Vector2 position, int time, HitTypes type, int beforeWait, List<string> objectParams) :
            base(position, time, type, beforeWait, objectParams)
        {
        }
    }

    [Flags] public enum HitTypes
    {
        HitCircle = 1 << 0,
        Slider = 1 << 1,
        NewCombo = 1 << 2,
        Spinner = 1 << 3,
        ColorHax1 = 1 << 4,
        ColorHax2 = 1 << 5,
        ColorHax3 = 1 << 6,
        OsuManiaHoldNote = 1 << 7
    }
}