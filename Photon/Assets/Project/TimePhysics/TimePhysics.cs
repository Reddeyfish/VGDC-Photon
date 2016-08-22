using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

public static class TimePhysics {

    public static List<TimeCollider> timePhysicsColliders = new List<TimeCollider>();

    public static bool SphereIntersection(Vector3 spherePos, float radius, Vector3 rayOrigin, Vector3 rayDirection)
    {
        //http://paulbourke.net/geometry/circlesphere/index.html#linesphere

        //a = 1, since direction is a unit vector

        float b = 2 * (Vector3.Dot(rayDirection, rayOrigin - spherePos));

        float c = spherePos.sqrMagnitude + rayOrigin.sqrMagnitude 
            - 2 * (Vector3.Dot(spherePos, rayOrigin))
            - (radius * radius);

        float result = (b * b) - (4 * c);

        return result >= 0;
    }

    public static bool SphereIntersection(Vector3 spherePos, float radius, Ray ray)
    {
        return SphereIntersection(spherePos, radius, ray.origin, ray.direction);
    }

    public static List<TimeCollider> RaycastAll(Vector3 rayOrigin, Vector3 rayUnitDirection, double time, TimePhysicsLayers layermask = TimePhysicsLayers.ALL)
    {
        Profiler.BeginSample("TimePhysicsRaycastAll");
        List<TimeCollider> results = new List<TimeCollider>();
        foreach (TimeCollider target in timePhysicsColliders)
        {
            if ((target.layer & layermask) == TimePhysicsLayers.NONE)
            {
                continue;
            }

            if (target.raycastHit(rayOrigin, rayUnitDirection, time))
            {
                results.Add(target);
            }
        }

        Profiler.EndSample();

        return results;
    }

    public static List<TimeCollider> RaycastAll(Ray ray, double time, TimePhysicsLayers layermask = TimePhysicsLayers.ALL)
    {
        return RaycastAll(ray.origin, ray.direction, time, layermask);
    }
}