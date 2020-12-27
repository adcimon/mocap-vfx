using System.Collections.Generic;
using UnityEngine;

public class Avatar : MonoBehaviour
{
    public bool showSkeleton = true;
    public Material skeletonMaterial;
    public float skeletonX;
    public float skeletonY;
    public float skeletonZ;
    public float skeletonScale = 1;
    private GameObject skeleton;
    private List<Skeleton> skeletons = new List<Skeleton>();

    public GameObject nose;
    public JointPoint[] jointPoints;

    private Animator animator;
    private Vector3 initPosition;
    private float centerTall = 224 * 0.75f;
    private float tall = 224 * 0.75f;
    private float prevTall = 224 * 0.75f;
    public float zScale = 0.8f;

    private void Start()
    {
        skeleton = new GameObject("Skeleton");
        animator = this.GetComponent<Animator>();
    }

    private void Update()
    {
        if( jointPoints != null )
        {
            PoseUpdate();
        }
    }

    public JointPoint[] Initialize()
    {
        jointPoints = new JointPoint[PositionIndex.Count.Int()];
        for( int i = 0; i < PositionIndex.Count.Int(); i++ )
        {
            jointPoints[i] = new JointPoint();
        }

        // Right arm.
        jointPoints[PositionIndex.RightShoulderBend.Int()].transform = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        jointPoints[PositionIndex.RightForearmBend.Int()].transform = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        jointPoints[PositionIndex.RightHand.Int()].transform = animator.GetBoneTransform(HumanBodyBones.RightHand);
        jointPoints[PositionIndex.RightThumb2.Int()].transform = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        jointPoints[PositionIndex.RightMid1.Int()].transform = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);

        // Left arm.
        jointPoints[PositionIndex.LeftShoulderBend.Int()].transform = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        jointPoints[PositionIndex.LeftForearmBend.Int()].transform = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        jointPoints[PositionIndex.LeftHand.Int()].transform = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        jointPoints[PositionIndex.LeftThumb2.Int()].transform = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        jointPoints[PositionIndex.LeftMid1.Int()].transform = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);

        // Face.
        jointPoints[PositionIndex.LeftEar.Int()].transform = animator.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.LeftEye.Int()].transform = animator.GetBoneTransform(HumanBodyBones.LeftEye);
        jointPoints[PositionIndex.RightEar.Int()].transform = animator.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.RightEye.Int()].transform = animator.GetBoneTransform(HumanBodyBones.RightEye);
        jointPoints[PositionIndex.Nose.Int()].transform = nose.transform;

        // Right leg.
        jointPoints[PositionIndex.RightThighBend.Int()].transform = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        jointPoints[PositionIndex.RightShin.Int()].transform = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        jointPoints[PositionIndex.RightFoot.Int()].transform = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        jointPoints[PositionIndex.RightToe.Int()].transform = animator.GetBoneTransform(HumanBodyBones.RightToes);

        // Left leg.
        jointPoints[PositionIndex.LeftThighBend.Int()].transform = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        jointPoints[PositionIndex.LeftShin.Int()].transform = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        jointPoints[PositionIndex.LeftFoot.Int()].transform = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        jointPoints[PositionIndex.LeftToe.Int()].transform = animator.GetBoneTransform(HumanBodyBones.LeftToes);

        // Etc.
        jointPoints[PositionIndex.AbdomenUpper.Int()].transform = animator.GetBoneTransform(HumanBodyBones.Spine);
        jointPoints[PositionIndex.Hip.Int()].transform = animator.GetBoneTransform(HumanBodyBones.Hips);
        jointPoints[PositionIndex.Head.Int()].transform = animator.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.Neck.Int()].transform = animator.GetBoneTransform(HumanBodyBones.Neck);
        jointPoints[PositionIndex.Spine.Int()].transform = animator.GetBoneTransform(HumanBodyBones.Spine);

        // Child settings.

        // Right arm.
        jointPoints[PositionIndex.RightShoulderBend.Int()].child = jointPoints[PositionIndex.RightForearmBend.Int()];
        jointPoints[PositionIndex.RightForearmBend.Int()].child = jointPoints[PositionIndex.RightHand.Int()];
        jointPoints[PositionIndex.RightForearmBend.Int()].parent = jointPoints[PositionIndex.RightShoulderBend.Int()];

        // Left arm.
        jointPoints[PositionIndex.LeftShoulderBend.Int()].child = jointPoints[PositionIndex.LeftForearmBend.Int()];
        jointPoints[PositionIndex.LeftForearmBend.Int()].child = jointPoints[PositionIndex.LeftHand.Int()];
        jointPoints[PositionIndex.LeftForearmBend.Int()].parent = jointPoints[PositionIndex.LeftShoulderBend.Int()];

        // Fase.

        // Right leg.
        jointPoints[PositionIndex.RightThighBend.Int()].child = jointPoints[PositionIndex.RightShin.Int()];
        jointPoints[PositionIndex.RightShin.Int()].child = jointPoints[PositionIndex.RightFoot.Int()];
        jointPoints[PositionIndex.RightFoot.Int()].child = jointPoints[PositionIndex.RightToe.Int()];
        jointPoints[PositionIndex.RightFoot.Int()].parent = jointPoints[PositionIndex.RightShin.Int()];

        // Left leg.
        jointPoints[PositionIndex.LeftThighBend.Int()].child = jointPoints[PositionIndex.LeftShin.Int()];
        jointPoints[PositionIndex.LeftShin.Int()].child = jointPoints[PositionIndex.LeftFoot.Int()];
        jointPoints[PositionIndex.LeftFoot.Int()].child = jointPoints[PositionIndex.LeftToe.Int()];
        jointPoints[PositionIndex.LeftFoot.Int()].parent = jointPoints[PositionIndex.LeftShin.Int()];

        // Etc.
        jointPoints[PositionIndex.Spine.Int()].child = jointPoints[PositionIndex.Neck.Int()];
        jointPoints[PositionIndex.Neck.Int()].child = jointPoints[PositionIndex.Head.Int()];
        //jointPoints[PositionIndex.head.Int()].child = jointPoints[PositionIndex.Nose.Int()];

        if( showSkeleton )
        {
            // Line child settings.

            // Right arm.
            AddSkeleton(PositionIndex.RightShoulderBend, PositionIndex.RightForearmBend);
            AddSkeleton(PositionIndex.RightForearmBend, PositionIndex.RightHand);
            AddSkeleton(PositionIndex.RightHand, PositionIndex.RightThumb2);
            AddSkeleton(PositionIndex.RightHand, PositionIndex.RightMid1);

            // Left arm.
            AddSkeleton(PositionIndex.LeftShoulderBend, PositionIndex.LeftForearmBend);
            AddSkeleton(PositionIndex.LeftForearmBend, PositionIndex.LeftHand);
            AddSkeleton(PositionIndex.LeftHand, PositionIndex.LeftThumb2);
            AddSkeleton(PositionIndex.LeftHand, PositionIndex.LeftMid1);

            // Fase.
            AddSkeleton(PositionIndex.LeftEar, PositionIndex.Nose);
            AddSkeleton(PositionIndex.RightEar, PositionIndex.Nose);

            // Right leg.
            AddSkeleton(PositionIndex.RightThighBend, PositionIndex.RightShin);
            AddSkeleton(PositionIndex.RightShin, PositionIndex.RightFoot);
            AddSkeleton(PositionIndex.RightFoot, PositionIndex.RightToe);

            // Left leg.
            AddSkeleton(PositionIndex.LeftThighBend, PositionIndex.LeftShin);
            AddSkeleton(PositionIndex.LeftShin, PositionIndex.LeftFoot);
            AddSkeleton(PositionIndex.LeftFoot, PositionIndex.LeftToe);

            // Etc.
            AddSkeleton(PositionIndex.Spine, PositionIndex.Neck);
            AddSkeleton(PositionIndex.Neck, PositionIndex.Head);
            AddSkeleton(PositionIndex.Head, PositionIndex.Nose);
            AddSkeleton(PositionIndex.Neck, PositionIndex.RightShoulderBend);
            AddSkeleton(PositionIndex.Neck, PositionIndex.LeftShoulderBend);
            AddSkeleton(PositionIndex.RightThighBend, PositionIndex.RightShoulderBend);
            AddSkeleton(PositionIndex.LeftThighBend, PositionIndex.LeftShoulderBend);
            AddSkeleton(PositionIndex.RightShoulderBend, PositionIndex.AbdomenUpper);
            AddSkeleton(PositionIndex.LeftShoulderBend, PositionIndex.AbdomenUpper);
            AddSkeleton(PositionIndex.RightThighBend, PositionIndex.AbdomenUpper);
            AddSkeleton(PositionIndex.LeftThighBend, PositionIndex.AbdomenUpper);
            AddSkeleton(PositionIndex.LeftThighBend, PositionIndex.RightThighBend);
        }

        // Set inverse.
        Vector3 forward = TriangleNormal(jointPoints[PositionIndex.Hip.Int()].transform.position, jointPoints[PositionIndex.LeftThighBend.Int()].transform.position, jointPoints[PositionIndex.RightThighBend.Int()].transform.position);
        foreach( JointPoint jointPoint in jointPoints )
        {
            if( jointPoint.transform != null )
            {
                jointPoint.initRotation = jointPoint.transform.rotation;
            }

            if( jointPoint.child != null )
            {
                jointPoint.inverse = GetInverse(jointPoint, jointPoint.child, forward);
                jointPoint.inverseRotation = jointPoint.inverse * jointPoint.initRotation;
            }
        }

        JointPoint hip = jointPoints[PositionIndex.Hip.Int()];
        initPosition = jointPoints[PositionIndex.Hip.Int()].transform.position;
        hip.inverse = Quaternion.Inverse(Quaternion.LookRotation(forward));
        hip.inverseRotation = hip.inverse * hip.initRotation;

        // Head rotation.
        JointPoint head = jointPoints[PositionIndex.Head.Int()];
        head.initRotation = jointPoints[PositionIndex.Head.Int()].transform.rotation;
        Vector3 gaze = jointPoints[PositionIndex.Nose.Int()].transform.position - jointPoints[PositionIndex.Head.Int()].transform.position;
        head.inverse = Quaternion.Inverse(Quaternion.LookRotation(gaze));
        head.inverseRotation = head.inverse * head.initRotation;

        JointPoint lHand = jointPoints[PositionIndex.LeftHand.Int()];
        Vector3 lf = TriangleNormal(lHand.pos3D, jointPoints[PositionIndex.LeftMid1.Int()].pos3D, jointPoints[PositionIndex.LeftThumb2.Int()].pos3D);
        lHand.initRotation = lHand.transform.rotation;
        lHand.inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.LeftThumb2.Int()].transform.position - jointPoints[PositionIndex.LeftMid1.Int()].transform.position, lf));
        lHand.inverseRotation = lHand.inverse * lHand.initRotation;

        JointPoint rHand = jointPoints[PositionIndex.RightHand.Int()];
        Vector3 rf = TriangleNormal(rHand.pos3D, jointPoints[PositionIndex.RightThumb2.Int()].pos3D, jointPoints[PositionIndex.RightMid1.Int()].pos3D);
        rHand.initRotation = jointPoints[PositionIndex.RightHand.Int()].transform.rotation;
        rHand.inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.RightThumb2.Int()].transform.position - jointPoints[PositionIndex.RightMid1.Int()].transform.position, rf));
        rHand.inverseRotation = rHand.inverse * rHand.initRotation;

        jointPoints[PositionIndex.Hip.Int()].score3D = 1f;
        jointPoints[PositionIndex.Neck.Int()].score3D = 1f;
        jointPoints[PositionIndex.Nose.Int()].score3D = 1f;
        jointPoints[PositionIndex.Head.Int()].score3D = 1f;
        jointPoints[PositionIndex.Spine.Int()].score3D = 1f;

        return jointPoints;
    }

    public void PoseUpdate()
    {
        // Calculate movement range of z-coordinate from height.
        float t1 = Vector3.Distance(jointPoints[PositionIndex.Head.Int()].pos3D, jointPoints[PositionIndex.Neck.Int()].pos3D);
        float t2 = Vector3.Distance(jointPoints[PositionIndex.Neck.Int()].pos3D, jointPoints[PositionIndex.Spine.Int()].pos3D);
        Vector3 pm = (jointPoints[PositionIndex.RightThighBend.Int()].pos3D + jointPoints[PositionIndex.LeftThighBend.Int()].pos3D) / 2f;
        float t3 = Vector3.Distance(jointPoints[PositionIndex.Spine.Int()].pos3D, pm);
        float t4r = Vector3.Distance(jointPoints[PositionIndex.RightThighBend.Int()].pos3D, jointPoints[PositionIndex.RightShin.Int()].pos3D);
        float t4l = Vector3.Distance(jointPoints[PositionIndex.LeftThighBend.Int()].pos3D, jointPoints[PositionIndex.LeftShin.Int()].pos3D);
        float t4 = (t4r + t4l) / 2f;
        float t5r = Vector3.Distance(jointPoints[PositionIndex.RightShin.Int()].pos3D, jointPoints[PositionIndex.RightFoot.Int()].pos3D);
        float t5l = Vector3.Distance(jointPoints[PositionIndex.LeftShin.Int()].pos3D, jointPoints[PositionIndex.LeftFoot.Int()].pos3D);
        float t5 = (t5r + t5l) / 2f;
        float t = t1 + t2 + t3 + t4 + t5;

        // Low pass filter in z direction.
        tall = t * 0.7f + prevTall * 0.3f;
        prevTall = tall;

        if( tall == 0 )
        {
            tall = centerTall;
        }

        float dz = (centerTall - tall) / centerTall * zScale;

        // Movement and rotatation of center.
        Vector3 forward = TriangleNormal(jointPoints[PositionIndex.Hip.Int()].pos3D, jointPoints[PositionIndex.LeftThighBend.Int()].pos3D, jointPoints[PositionIndex.RightThighBend.Int()].pos3D);
        jointPoints[PositionIndex.Hip.Int()].transform.position = jointPoints[PositionIndex.Hip.Int()].pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz);
        jointPoints[PositionIndex.Hip.Int()].transform.rotation = Quaternion.LookRotation(forward) * jointPoints[PositionIndex.Hip.Int()].inverseRotation;

        // Rotate each of the bones.
        foreach( JointPoint jointPoint in jointPoints )
        {
            if( jointPoint.parent != null )
            {
                Vector3 fv = jointPoint.parent.pos3D - jointPoint.pos3D;
                jointPoint.transform.rotation = Quaternion.LookRotation(jointPoint.pos3D - jointPoint.child.pos3D, fv) * jointPoint.inverseRotation;
            }
            else if( jointPoint.child != null )
            {
                jointPoint.transform.rotation = Quaternion.LookRotation(jointPoint.pos3D - jointPoint.child.pos3D, forward) * jointPoint.inverseRotation;
            }
        }

        // Head rotation.
        Vector3 gaze = jointPoints[PositionIndex.Nose.Int()].pos3D - jointPoints[PositionIndex.Head.Int()].pos3D;
        Vector3 f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].pos3D, jointPoints[PositionIndex.RightEar.Int()].pos3D, jointPoints[PositionIndex.LeftEar.Int()].pos3D);
        JointPoint head = jointPoints[PositionIndex.Head.Int()];
        head.transform.rotation = Quaternion.LookRotation(gaze, f) * head.inverseRotation;
        
        // Wrist rotation (test).
        JointPoint lHand = jointPoints[PositionIndex.LeftHand.Int()];
        Vector3 lf = TriangleNormal(lHand.pos3D, jointPoints[PositionIndex.LeftMid1.Int()].pos3D, jointPoints[PositionIndex.LeftThumb2.Int()].pos3D);
        lHand.transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.LeftThumb2.Int()].pos3D - jointPoints[PositionIndex.LeftMid1.Int()].pos3D, lf) * lHand.inverseRotation;

        JointPoint rHand = jointPoints[PositionIndex.RightHand.Int()];
        Vector3 rf = TriangleNormal(rHand.pos3D, jointPoints[PositionIndex.RightThumb2.Int()].pos3D, jointPoints[PositionIndex.RightMid1.Int()].pos3D);
        //rHand.transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.RightThumb2.Int()].pos3D - jointPoints[PositionIndex.RightMid1.Int()].pos3D, rf) * rHand.inverseRotation;
        rHand.transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.RightThumb2.Int()].pos3D - jointPoints[PositionIndex.RightMid1.Int()].pos3D, rf) * rHand.inverseRotation;

        foreach( Skeleton sk in skeletons )
        {
            JointPoint s = sk.start;
            JointPoint e = sk.end;

            sk.lineRenderer.SetPosition(0, new Vector3(s.pos3D.x * skeletonScale + skeletonX, s.pos3D.y * skeletonScale + skeletonY, s.pos3D.z * skeletonScale + skeletonZ));
            sk.lineRenderer.SetPosition(1, new Vector3(e.pos3D.x * skeletonScale + skeletonX, e.pos3D.y * skeletonScale + skeletonY, e.pos3D.z * skeletonScale + skeletonZ));
        }
    }

    private Vector3 TriangleNormal( Vector3 a, Vector3 b, Vector3 c )
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }

    private Quaternion GetInverse( JointPoint p1, JointPoint p2, Vector3 forward )
    {
        return Quaternion.Inverse(Quaternion.LookRotation(p1.transform.position - p2.transform.position, forward));
    }

    private void AddSkeleton( PositionIndex s, PositionIndex e )
    {
        Skeleton sk = new Skeleton()
        {
            gameObject = new GameObject("Line"),
            start = jointPoints[s.Int()],
            end = jointPoints[e.Int()],
        };

        sk.gameObject.transform.SetParent(skeleton.transform);
        sk.lineRenderer = sk.gameObject.AddComponent<LineRenderer>();
        sk.lineRenderer.startWidth = 0.04f;
        sk.lineRenderer.endWidth = 0.01f;
        
        // Define the number of vertices.
        sk.lineRenderer.positionCount = 2;
        sk.lineRenderer.material = skeletonMaterial;

        skeletons.Add(sk);
    }
}