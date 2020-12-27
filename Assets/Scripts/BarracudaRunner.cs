using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class BarracudaRunner : MonoBehaviour
{
    public NNModel neuralNetworkModel;
    public WorkerFactory.Type workerType = WorkerFactory.Type.Auto;
    public float waitTime = 10;
    public bool verbose = true;

    /// <summary>
    /// Coordinates of joint points.
    /// </summary>
    private JointPoint[] jointPoints;

    /// <summary>
    /// Number of joint points.
    /// </summary>
    private const int jointNum = 24;
    private int jointNumSquared { get { return jointNum * 2; } }
    private int jointNumCube { get { return jointNum * 3; } }

    /// <summary>
    /// Input image size.
    /// </summary>
    public int inputImageSize;
    private float inputImageHalfSize { get { return (float)inputImageSize / 2f; } }
    private float imageScale { get { return inputImageSize / (float)heatMapCol; } } // 224f / (float)InputImageSize;

    /// <summary>
    /// Column number of heatmap.
    /// </summary>
    public int heatMapCol;
    private int heatMapColSquared { get { return heatMapCol * heatMapCol; } }
    private int heatMapColCube { get { return heatMapCol * heatMapCol * heatMapCol; } }

    /// <summary>
    /// Kalman filter parameter Q.
    /// </summary>
    public float kalmanParamQ;

    /// <summary>
    /// Kalman filter parameter R.
    /// </summary>
    public float kalmanParamR;

    public bool useLowPassFilter = true;
    public float lowPassParam = 0.1f;

    public Texture2D baseTexture;
    public VideoCapture videoCapture;
    public Avatar avatar;

    private float[] heatMap3D;
    private float[] offset3D;
    private int heatMapColxJointNum { get { return heatMapCol * jointNum; } }
    private int cubeOffsetLinear { get { return heatMapCol * jointNumCube; } }
    private int cubeOffsetSquared { get { return heatMapColSquared * jointNumCube; } }

    private const string inputName1 = "input.1";
    private const string inputName2 = "input.4";
    private const string inputName3 = "input.7";
    private Tensor input = new Tensor();
    private Dictionary<string, Tensor> inputs = new Dictionary<string, Tensor>() { { inputName1, null }, { inputName2, null }, { inputName3, null }, };
    private Tensor[] outputs = new Tensor[4];

    private Model model;
    private IWorker worker;
    private bool loaded = false;

    private void Start()
    {
        heatMap3D = new float[jointNum * heatMapColCube];
        offset3D = new float[jointNum * heatMapColCube * 3];

        // Disabel sleep.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Initialize model.
        model = ModelLoader.Load(neuralNetworkModel, verbose);
        worker = WorkerFactory.CreateWorker(workerType, model, verbose);

        StartCoroutine(Load());
    }

    private void Update()
    {
        if( loaded )
        {
            UpdateAvatar();
        }
    }

    private IEnumerator Load()
    {
        inputs[inputName1] = new Tensor(baseTexture);
        inputs[inputName2] = new Tensor(baseTexture);
        inputs[inputName3] = new Tensor(baseTexture);

        // Create input and execute model.
        yield return worker.StartManualSchedule(inputs);

        // Get outputs.
        for( int i = 2; i < model.outputs.Count; i++ )
        {
            outputs[i] = worker.PeekOutput(model.outputs[i]);
        }

        // Get data from outputs.
        offset3D = outputs[2].data.Download(outputs[2].shape);
        heatMap3D = outputs[3].data.Download(outputs[3].shape);

        // Release outputs.
        for( int i = 2; i < outputs.Length; i++ )
        {
            outputs[i].Dispose();
        }

        jointPoints = avatar.Initialize();

        PredictPose();

        yield return new WaitForSeconds(waitTime);

        videoCapture.Initialize(inputImageSize, inputImageSize);

        loaded = true;
    }

    private void UpdateAvatar()
    {
        input = new Tensor(videoCapture.renderTexture);
        if( inputs[inputName1] == null )
        {
            inputs[inputName1] = input;
            inputs[inputName2] = new Tensor(videoCapture.renderTexture);
            inputs[inputName3] = new Tensor(videoCapture.renderTexture);
        }
        else
        {
            inputs[inputName3].Dispose();

            inputs[inputName3] = inputs[inputName2];
            inputs[inputName2] = inputs[inputName1];
            inputs[inputName1] = input;
        }

        StartCoroutine(ExecuteModel());
    }

    private IEnumerator ExecuteModel()
    {
        // Create input and execute model.
        yield return worker.StartManualSchedule(inputs);

        // Get outputs.
        for( int i = 2; i < model.outputs.Count; i++ )
        {
            outputs[i] = worker.PeekOutput(model.outputs[i]);
        }

        // Get data from outputs.
        offset3D = outputs[2].data.Download(outputs[2].shape);
        heatMap3D = outputs[3].data.Download(outputs[3].shape);
        
        // Release outputs.
        for( int i = 2; i < outputs.Length; i++ )
        {
            outputs[i].Dispose();
        }

        PredictPose();
    }

    /// <summary>
    /// Predict positions of each of joints based on the inference.
    /// </summary>
    private void PredictPose()
    {
        for( int j = 0; j < jointNum; j++ )
        {
            int maxXIndex = 0;
            int maxYIndex = 0;
            int maxZIndex = 0;
            jointPoints[j].score3D = 0.0f;
            int jj = j * heatMapCol;
            for( int z = 0; z < heatMapCol; z++ )
            {
                int zz = jj + z;
                for( int y = 0; y < heatMapCol; y++ )
                {
                    int yy = y * heatMapColSquared * jointNum + zz;
                    for( int x = 0; x < heatMapCol; x++ )
                    {
                        float v = heatMap3D[yy + x * heatMapColxJointNum];
                        if( v > jointPoints[j].score3D )
                        {
                            jointPoints[j].score3D = v;
                            maxXIndex = x;
                            maxYIndex = y;
                            maxZIndex = z;
                        }
                    }
                }
            }
           
            jointPoints[j].Now3D.x = (offset3D[maxYIndex * cubeOffsetSquared + maxXIndex * cubeOffsetLinear + j * heatMapCol + maxZIndex] + 0.5f + (float)maxXIndex) * imageScale - inputImageHalfSize;
            jointPoints[j].Now3D.y = inputImageHalfSize - (offset3D[maxYIndex * cubeOffsetSquared + maxXIndex * cubeOffsetLinear + (j + jointNum) * heatMapCol + maxZIndex] + 0.5f + (float)maxYIndex) * imageScale;
            jointPoints[j].Now3D.z = (offset3D[maxYIndex * cubeOffsetSquared + maxXIndex * cubeOffsetLinear + (j + jointNumSquared) * heatMapCol + maxZIndex] + 0.5f + (float)(maxZIndex - 14)) * imageScale;
        }

        // Calculate hip location.
        Vector3 lc = (jointPoints[PositionIndex.rThighBend.Int()].Now3D + jointPoints[PositionIndex.lThighBend.Int()].Now3D) / 2f;
        jointPoints[PositionIndex.hip.Int()].Now3D = (jointPoints[PositionIndex.abdomenUpper.Int()].Now3D + lc) / 2f;

        // Calculate neck location.
        jointPoints[PositionIndex.neck.Int()].Now3D = (jointPoints[PositionIndex.rShldrBend.Int()].Now3D + jointPoints[PositionIndex.lShldrBend.Int()].Now3D) / 2f;

        // Calculate head location.
        Vector3 cEar = (jointPoints[PositionIndex.rEar.Int()].Now3D + jointPoints[PositionIndex.lEar.Int()].Now3D) / 2f;
        Vector3 hv = cEar - jointPoints[PositionIndex.neck.Int()].Now3D;
        Vector3 nhv = Vector3.Normalize(hv);
        Vector3 nv = jointPoints[PositionIndex.Nose.Int()].Now3D - jointPoints[PositionIndex.neck.Int()].Now3D;
        jointPoints[PositionIndex.head.Int()].Now3D = jointPoints[PositionIndex.neck.Int()].Now3D + nhv * Vector3.Dot(nhv, nv);

        // Calculate spine location.
        jointPoints[PositionIndex.spine.Int()].Now3D = jointPoints[PositionIndex.abdomenUpper.Int()].Now3D;

        // Kalman filter.
        foreach( JointPoint jp in jointPoints )
        {
            kalmanFilter(jp);
        }

        // Low pass filter.
        if( useLowPassFilter )
        {
            foreach( JointPoint jp in jointPoints )
            {
                jp.PrevPos3D[0] = jp.Pos3D;
                for( int i = 1; i < jp.PrevPos3D.Length; i++ )
                {
                    jp.PrevPos3D[i] = jp.PrevPos3D[i] * lowPassParam + jp.PrevPos3D[i - 1] * (1f - lowPassParam);
                }

                jp.Pos3D = jp.PrevPos3D[jp.PrevPos3D.Length - 1];
            }
        }
    }

    private void kalmanFilter( JointPoint measurement )
    {
        measurementUpdate(measurement);
        measurement.Pos3D.x = measurement.X.x + (measurement.Now3D.x - measurement.X.x) * measurement.K.x;
        measurement.Pos3D.y = measurement.X.y + (measurement.Now3D.y - measurement.X.y) * measurement.K.y;
        measurement.Pos3D.z = measurement.X.z + (measurement.Now3D.z - measurement.X.z) * measurement.K.z;
        measurement.X = measurement.Pos3D;
    }

	private void measurementUpdate( JointPoint measurement )
    {
        measurement.K.x = (measurement.P.x + kalmanParamQ) / (measurement.P.x + kalmanParamQ + kalmanParamR);
        measurement.K.y = (measurement.P.y + kalmanParamQ) / (measurement.P.y + kalmanParamQ + kalmanParamR);
        measurement.K.z = (measurement.P.z + kalmanParamQ) / (measurement.P.z + kalmanParamQ + kalmanParamR);
        measurement.P.x = kalmanParamR * (measurement.P.x + kalmanParamQ) / (kalmanParamR + measurement.P.x + kalmanParamQ);
        measurement.P.y = kalmanParamR * (measurement.P.y + kalmanParamQ) / (kalmanParamR + measurement.P.y + kalmanParamQ);
        measurement.P.z = kalmanParamR * (measurement.P.z + kalmanParamQ) / (kalmanParamR + measurement.P.z + kalmanParamQ);
    }
}