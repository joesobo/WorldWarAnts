using UnityEngine;

public struct Triangle {
#pragma warning disable 649 // disable unassigned variable warning
    public Vector2 A;
    public Vector2 B;
    public Vector2 C;
    public readonly float Red;
    public readonly float Green;
    public readonly float Blue;

    public Triangle(Vector2 a, Vector2 b, Vector2 c, float red, float green, float blue) {
        A = a;
        B = b;
        C = c;
        Red = red;
        Green = green;
        Blue = blue;
    }

    public Vector2 this[int i] {
        get {
            return i switch {
                0 => A,
                1 => B,
                _ => C
            };
        }
    }
}