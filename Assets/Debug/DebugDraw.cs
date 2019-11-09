using UnityEngine;

public class DebugDraw {
    public static void WireBox(Vector2 position, Vector2 size, float angle, Color color, float duration) {
        var eVectorX = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));
        var eVectorY = new Vector2(-eVectorX.y, eVectorX.x);

        var crossVector = eVectorX * size.x + eVectorY * size.y;
        var topRightPosition = position + crossVector / 2;

        Debug.DrawLine(topRightPosition, topRightPosition - eVectorX * size.x, color, duration);
        Debug.DrawLine(topRightPosition - eVectorX * size.x, topRightPosition - crossVector, color, duration);
        Debug.DrawLine(topRightPosition - crossVector, topRightPosition - eVectorY * size.y, color, duration);
        Debug.DrawLine(topRightPosition - eVectorY * size.y, topRightPosition, color, duration);
    }

    public static void WireBox(Vector2 position, float size, float angle, Color color, float duration) {
        var eVectorX = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));
        var eVectorY = new Vector2(-eVectorX.y, eVectorX.x);

        var crossVector = eVectorX * size + eVectorY * size;
        var topRightPosition = position + crossVector / 2;

        Debug.DrawLine(topRightPosition, topRightPosition - eVectorX * size, color, duration);
        Debug.DrawLine(topRightPosition - eVectorX * size, topRightPosition - crossVector, color, duration);
        Debug.DrawLine(topRightPosition - crossVector, topRightPosition - eVectorY * size, color, duration);
        Debug.DrawLine(topRightPosition - eVectorY * size, topRightPosition, color, duration);
    }
}