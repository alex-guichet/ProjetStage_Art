using System;
using UnityEngine;

public static class MathHelper
{
    public static float Map( this float value, float from_min, float from_max, float to_min, float to_max )
    {
        return to_min + ( value - from_min ) * ( to_max - to_min ) / ( from_max - from_min );
    }

    public static float Distance(Component firstObject, Component secondObject)
    {
        return Vector3.Distance(firstObject.transform.position, secondObject.transform.position);
    }

    public static float GetGroundAngle( Vector3 normal, Vector3 direction )
    {
        return Vector3.SignedAngle( normal, direction, Vector3.Cross( normal, direction ) ) - 90.0f;
    }

    public static float Smooth( float current_value, float target_value, float sharpness )
    {
        return Mathf.Lerp( current_value, target_value, 1 - Mathf.Exp( -sharpness * Time.deltaTime ) );
    }

    public static Vector3 Smooth( Vector3 current_value, Vector3 target_value, float sharpness )
    {
        return Vector3.Lerp( current_value, target_value, 1 - Mathf.Exp( -sharpness * Time.deltaTime ) );
    }

    public static Quaternion Smooth( Quaternion current_value, Quaternion target_value, float sharpness )
    {
        return Quaternion.Slerp( current_value, target_value, 1 - Mathf.Exp( -sharpness * Time.deltaTime ) );
    }
    
    public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }
}

