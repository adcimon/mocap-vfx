public enum PositionIndex
{
    RightShoulderBend = 0,
    RightForearmBend,
    RightHand,
    RightThumb2,
    RightMid1,

    LeftShoulderBend,
    LeftForearmBend,
    LeftHand,
    LeftThumb2,
    LeftMid1,

    LeftEar,
    LeftEye,
    RightEar,
    RightEye,
    Nose,

    RightThighBend,
    RightShin,
    RightFoot,
    RightToe,

    LeftThighBend,
    LeftShin,
    LeftFoot,
    LeftToe,

    AbdomenUpper,

    // Calculated coordinates.
    Hip,
    Head,
    Neck,
    Spine,

    Count,
    None,
}

public static class PositionIndexExtension
{
    public static int Int( this PositionIndex i )
    {
        return (int)i;
    }
}