using UnityEngine;
using System.Collections;

public class TimeSphereCollider : TimeCollider
{
    /// <summary>
    /// Do not change at runtime; TimeCollider doesn't store scaling data
    /// </summary>
    [SerializeField]
    protected float radius = 1;

    public override bool raycastHit(Vector3 rayOrigin, Vector3 rayUnitDirection, double time, float distance = 2048f)
    {
        if (!TimeInRange(time))
        {
            return false;
        }
        else
        {
            return TimePhysics.InternalLogic.SphereLineSegmentIntersection(PositionAtTime(time), radius, rayOrigin, rayUnitDirection, distance);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        // Display the radius when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + offset, radius);
    }
}
