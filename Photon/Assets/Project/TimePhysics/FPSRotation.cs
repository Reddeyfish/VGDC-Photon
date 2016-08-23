using UnityEngine;
using System.Collections;

/// <summary>
/// A rotation in the X and Y axis only. No rolling, and no going upside down.
/// Used to serialize player rotations for networking.
/// </summary>
public class FPSRotation {

    public readonly float pitch;
    public readonly float yaw;

    public FPSRotation(float pitch = 0, float yaw = 0)
    {

        if (pitch > 180f)
        {
            pitch -= 360f;
        }
        pitch = Mathf.Clamp(pitch, -89.0f, 89.0f);


        this.pitch = pitch;
        this.yaw = yaw;
    }

    public static implicit operator FPSRotation(Quaternion quat)
    {
        Vector2 eulerAngles = quat.eulerAngles;
        FPSRotation result = new FPSRotation(eulerAngles.x, eulerAngles.y);
        return result;
    }

    public static implicit operator Quaternion(FPSRotation rot)
    {
        return Quaternion.Euler(rot.pitch, rot.yaw, 0);
    }

    public override string ToString()
    {
        return string.Format("(Pitch: {0}, Yaw: {1})", pitch, yaw);
    }
}
