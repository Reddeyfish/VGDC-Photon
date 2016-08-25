using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

public static class TimePhysics {

    public static class InternalLogic
    { //more for the namespacing than an actual class. These aren't supposed to be called directly from general gameplay logic, only from time-physics-related components

        public static bool SphereLineIntersection(Vector3 spherePos, float radius, Vector3 rayOrigin, Vector3 rayDirection)
        {
            //http://paulbourke.net/geometry/circlesphere/index.html#linesphere

            //a = 1, since direction is a unit vector
            Assert.IsTrue(rayDirection.sqrMagnitude == 1);
            rayDirection.Normalize();

            float b = 2 * (Vector3.Dot(rayDirection, rayOrigin - spherePos));

            float c = spherePos.sqrMagnitude + rayOrigin.sqrMagnitude
                - 2 * (Vector3.Dot(spherePos, rayOrigin))
                - (radius * radius);

            float result = (b * b) - (4 * c);

            return result >= 0;
        }

        public static bool SphereLineIntersection(Vector3 spherePos, float radius, Ray ray)
        {
            return SphereLineIntersection(spherePos, radius, ray.origin, ray.direction);
        }

        public static bool SphereLineSegmentIntersection(Vector3 spherePos, float radius, Vector3 rayOrigin, Vector3 rayDirection, float distance = 2048f)
        {
            Assert.IsTrue(rayDirection.sqrMagnitude == 1);
            rayDirection.Normalize();
            //This doesn't account for a few edge cases involving an endpoint inside the sphere.

            //http://paulbourke.net/geometry/circlesphere/index.html#linesphere
            

            //check if our segment has the possibility to intersect. U is interpolation value of the closest point (0 for rayOrigin, 1 for rayEnd)
            float u = Vector3.Dot(spherePos - rayOrigin, distance * rayDirection)
                / (distance * distance);

            if (u < 0 || u > 1)
            {
                return false; //closest point is not on the segment
            }
            return SphereLineIntersection(spherePos, radius, rayOrigin, rayDirection);
        }
    }


    public static List<TimeCollider> timePhysicsColliders = new List<TimeCollider>();

    public static List<TimeCollider> RaycastAll(Vector3 rayOrigin, Vector3 rayUnitDirection, double time, float distance = 2048f, TimePhysicsLayers layermask = TimePhysicsLayers.ALL)
    {
        Profiler.BeginSample("TimePhysicsRaycastAll");
        List<TimeCollider> results = new List<TimeCollider>();
        foreach (TimeCollider target in timePhysicsColliders)
        {
            if ((target.layer & layermask) == TimePhysicsLayers.NONE)
            {
                continue;
            }

            if (target.raycastHit(rayOrigin, rayUnitDirection, time, distance))
            {
                results.Add(target);
            }
        }

        Profiler.EndSample();

        return results;
    }

    public static List<TimeCollider> RaycastAll(Ray ray, double time, float distance = 2048f, TimePhysicsLayers layermask = TimePhysicsLayers.ALL)
    {
        return RaycastAll(ray.origin, ray.direction, time, distance, layermask);
    }
}