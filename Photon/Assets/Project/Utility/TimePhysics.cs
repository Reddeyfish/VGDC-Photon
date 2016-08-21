using UnityEngine;
using System.Collections;

public static class TimePhysics {

    public static bool SphereIntersection(Vector3 spherePos, float radius, Vector3 rayOrigin, Vector3 rayDirection)
    {
        //http://paulbourke.net/geometry/circlesphere/index.html#linesphere

        //a = 1, since direction is a unit vector

        float b = 2 * (Vector3.Dot(rayDirection, rayOrigin - spherePos));

        float c = spherePos.sqrMagnitude + rayOrigin.sqrMagnitude 
            - 2 * (Vector3.Dot(spherePos, rayOrigin))
            - (radius * radius);

        float result = (b * b) - (4 * rayDirection.magnitude * c);

        return result >= 0;
    }
}
