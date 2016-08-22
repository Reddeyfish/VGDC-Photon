using UnityEngine;
using System.Collections;

public class TimeSphereCollider : TimeCollider
{
    public Vector3 center;
    public float radius = 1;

    public override bool raycastHit(Vector3 rayOrigin, Vector3 rayUnitDirection, double time)
    {
        if (!TimeInRange(time))
        {
            return false;
        }
        else
        {
            return TimePhysics.SphereIntersection(PositionAtTime(time) + center, radius, rayOrigin, rayUnitDirection);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        // Display the radius when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + center, radius);
    }
}
