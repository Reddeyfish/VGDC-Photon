using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a stat. Treat it like a java enum.
/// </summary>
public class Stats {

    public enum StatFormat {INT, FLOAT};

    private static int counter = 0;
    public readonly int index;
    public readonly string displayName;
    public readonly string description;
    public readonly StatFormat format;

    private Stats(string displayName, string description, StatFormat format)
    {
        this.index = counter++;
        this.displayName = displayName;
        this.description = description;
        this.format = format;
        stats.Add(this);
    }

    public static implicit operator int(Stats stat) {return stat.index;}
    public static explicit operator Stats(int index)
    {
        return stats[index];
    }

    public override int GetHashCode()
    {
        return index;
    }

    private static readonly List<Stats> stats = new List<Stats>();

    //yes, this is safe. static objects are initialized in order.
    public static readonly Stats SHOTS_FIRED = new Stats("Shots Fired", "Number of projectiles fired", StatFormat.INT);
    public static readonly Stats SHOTS_HIT = new Stats("Shots Hit", "Number of projectiles which have hit a target", StatFormat.INT);
    public static readonly Stats DEATHS = new Stats("Deaths", "Number of times you have perished", StatFormat.INT);
    public static readonly Stats DAMAGE_DEALT = new Stats("Damage Dealt", "How much damage your actions have inflicted on your foes", StatFormat.FLOAT);
    public static readonly Stats DISTANCE_TRAVELLED = new Stats("Distance Travelled", "How far your adventures have taken you", StatFormat.FLOAT);
}
