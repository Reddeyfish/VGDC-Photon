using UnityEngine;
using System.Collections;

[System.Flags]
public enum TimePhysicsLayers
{
    NONE = 0,
    PLAYERS = 1,
    PROJECTILES = 2,
    ALL = 3
}