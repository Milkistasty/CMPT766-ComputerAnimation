// Name: Wenhe Wang
// Student ID: 301586596
// Email: wwa118@sfu.ca

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CatmullRom
{
    public static Vector2 Evaluate(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        /*** Please write your code here ***/

        // reference: https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
        //            and https://qroph.github.io/2018/07/30/smooth-paths-using-catmull-rom-splines.html

        // define a struct of the spline segment
        // p(0) = p1 = d,
        // p(1) = p2 = a + b + c + d,
        // p'(0) = m1 = c,
        // p'(1) = m2 = 3a + 2b + c 

        // after simplifying, we have 
        // a = 2p1 − 2p2 + m1 + m2,
        // b = −3p1 + 3p2 − 2m1 − m2,
        // c = m1,
        // d = p1

        // tension parameter that defines how “tight” the spline is.
        // When tension is 0, the result looks like the curve,
        // and when tension is 1, the result is straight lines
        float tension = 0f;

        // alpha is 0.5 which gives us a centripetal Catmull-Rom spline
        float alpha = 0.5f;

        // time diffs
        float t01 = Mathf.Pow(Vector2.Distance(p0, p1), alpha);
        float t12 = Mathf.Pow(Vector2.Distance(p1, p2), alpha);
        float t23 = Mathf.Pow(Vector2.Distance(p2, p3), alpha);

        // Tangents m1 and m2
        Vector2 m1 = (1.0f - tension) *
                    (p2 - p1 + t12 * ((p1 - p0) / t01 - (p2 - p0) / (t01 + t12)));
        Vector2 m2 = (1.0f - tension) *
                    (p2 - p1 + t12 * ((p3 - p2) / t23 - (p3 - p1) / (t12 + t23)));

        // Compute spline coefficients
        Vector2 a = 2f * p1 - 2f * p2 + m1 + m2;
        Vector2 b = -3f * p1 + 3f * p2 - 2f * m1 - m2;
        Vector2 c = m1;
        Vector2 d = p1;

        // applying the p(t) -> Catmull-Rom spline equation to calculate the point
        Vector2 point = a * Mathf.Pow(t, 3) + b * Mathf.Pow(t, 2) + c * t + d;

        return point;
    }
}
