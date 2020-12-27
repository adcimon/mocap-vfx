using System;
using UnityEngine;

[Serializable]
public class JointPoint
{
    public Vector2 pos2D = new Vector2();
    public float score2D;

    public Vector3 pos3D = new Vector3();
    public Vector3 now3D = new Vector3();
    public Vector3[] prevPos3D = new Vector3[6];
    public float score3D;

    // Bones.
    public Transform transform;
    public Quaternion initRotation;
    public Quaternion inverse;
    public Quaternion inverseRotation;

    public JointPoint child;
    public JointPoint parent;

    // Kalman filter.
    public Vector3 p = new Vector3();
    public Vector3 x = new Vector3();
    public Vector3 k = new Vector3();
}