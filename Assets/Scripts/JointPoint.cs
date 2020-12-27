using System;
using UnityEngine;

[Serializable]
public class JointPoint
{
    public Vector2 Pos2D = new Vector2();
    public float score2D;

    public Vector3 Pos3D = new Vector3();
    public Vector3 Now3D = new Vector3();
    public Vector3[] PrevPos3D = new Vector3[6];
    public float score3D;

    // Bones.
    public Transform Transform = null;
    public Quaternion InitRotation;
    public Quaternion Inverse;
    public Quaternion InverseRotation;

    public JointPoint Child = null;
    public JointPoint Parent = null;

    // For Kalman filter.
    public Vector3 P = new Vector3();
    public Vector3 X = new Vector3();
    public Vector3 K = new Vector3();
}