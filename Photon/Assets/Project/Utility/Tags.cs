using UnityEngine;
using System.Collections;


// for the autocomplete!

public class Tags{
    public const string player = "Player";
    public const string canvas = "Canvas";
    public const string untagged = "Untagged";
    public const string stage = "Stage";
    public const string puck = "Puck";
    public const string gameController = "GameController";

    public class Scenes
    {
        public const string root = "RootScene";
#if UNITY_EDITOR
        public const string derek = "Derek";
#endif
        public const string select = "PlayerRegistration";
    }

    //public class Axis
    //{
    //    //public const string horizontal = "Horizontal";
    //    //public const string vertical = "Vertical";
    //}

    public class Layers
    {
        public const string movable = "Movables";
        public const string stage = "Stage";
    }

    public class SortingLayers
    {
        public const string overlay = "Overlay";
    }

    public class AnimatorParams
    {
    }

    public class PlayerPrefKeys
    {
    }

    public class Options
    {
        public const string SoundLevel = "SoundLevel";
        public const string MusicLevel = "MusicLevel";
    }

    public class ShaderParams
    {
        public static int color = Shader.PropertyToID("_Color");
        public static int emission = Shader.PropertyToID("_EmissionColor");
        public static int cutoff = Shader.PropertyToID("_Cutoff");
        public static int noiseStrength = Shader.PropertyToID("_NoiseStrength");
        public static int effectTexture = Shader.PropertyToID("_EffectTex");
        public static int rangeMin = Shader.PropertyToID("_RangeMin");
        public static int rangeMax = Shader.PropertyToID("_RangeMax");
        public static int imageStrength = Shader.PropertyToID("_ImageStrength");
        public static int alpha = Shader.PropertyToID("_MainTexAlpha");
    }
}