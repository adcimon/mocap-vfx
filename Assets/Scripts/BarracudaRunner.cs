using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class BarracudaRunner : MonoBehaviour
{
    public NNModel neuralNetworkModel;
    public WorkerFactory.Type workerType = WorkerFactory.Type.Auto;
    public bool verbose = true;

    /// <summary>
    /// Coordinates of joint points.
    /// </summary>
    private Avatar.JointPoint[] jointPoints;
    
    /// <summary>
    /// Number of joint points
    /// </summary>
    private const int JointNum = 24;

    /// <summary>
    /// Input image size.
    /// </summary>
    public int InputImageSize;

    /// <summary>
    /// Input image half size.
    /// </summary>
    private float InputImageSizeHalf;

    /// <summary>
    /// column number of heatmap
    /// </summary>
    public int HeatMapCol;
    private float InputImageSizeF;

    /// <summary>
    /// Column number of heatmap in 2D image
    /// </summary>
    private int HeatMapCol_Squared;
    
    /// <summary>
    /// Column nuber of heatmap in 3D model
    /// </summary>
    private int HeatMapCol_Cube;
    private float ImageScale;
    
    /// <summary>
    /// Buffer memory has 3D heat map
    /// </summary>
    private float[] heatMap3D;
    
    /// <summary>
    /// Buffer memory hash 3D offset
    /// </summary>
    private float[] offset3D;
    
    /// <summary>
    /// Number of joints in 2D image
    /// </summary>
    private int JointNum_Squared = JointNum * 2;
    
    /// <summary>
    /// Number of joints in 3D model
    /// </summary>
    private int JointNum_Cube = JointNum * 3;

    /// <summary>
    /// HeatMapCol * JointNum
    /// </summary>
    private int HeatMapCol_JointNum;

    /// <summary>
    /// HeatMapCol * JointNum_Squared
    /// </summary>
    private int CubeOffsetLinear;

    /// <summary>
    /// HeatMapCol * JointNum_Cube
    /// </summary>
    private int CubeOffsetSquared;

    public float KalmanParamQ;
    public float KalmanParamR;

    public bool useLowPassFilter = true;
    public float lowPassParam = 0.1f;

    public float waitTimeModelLoad = 10;
    public Texture2D baseTexture;
    public VideoCapture videoCapture;
    public Avatar avatar;

    private Model model;
    private IWorker worker;
    private bool loaded = false;

    private const string inputName1 = "input.1";
    private const string inputName2 = "input.4";
    private const string inputName3 = "input.7";
    //private const string inputName1 = "0";
    //private const string inputName2 = "1";
    //private const string inputName3 = "2";
    private Tensor input = new Tensor();
    private Dictionary<string, Tensor> inputs = new Dictionary<string, Tensor>() { { inputName1, null }, { inputName2, null }, { inputName3, null }, };
    private Tensor[] outputs = new Tensor[4];

    private void Start()
    {
        // Initialize.
        HeatMapCol_Squared = HeatMapCol * HeatMapCol;
        HeatMapCol_Cube = HeatMapCol * HeatMapCol * HeatMapCol;
        HeatMapCol_JointNum = HeatMapCol * JointNum;
        CubeOffsetLinear = HeatMapCol * JointNum_Cube;
        CubeOffsetSquared = HeatMapCol_Squared * JointNum_Cube;

        heatMap3D = new float[JointNum * HeatMapCol_Cube];
        offset3D = new float[JointNum * HeatMapCol_Cube * 3];
        InputImageSizeF = InputImageSize;
        InputImageSizeHalf = InputImageSizeF / 2f;
        ImageScale = InputImageSize / (float)HeatMapCol; // 224f / (float)InputImageSize;

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

        yield return new WaitForSeconds(waitTimeModelLoad);

        videoCapture.Initialize(InputImageSize, InputImageSize);

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

        StartCoroutine(ExecuteModelAsync());
    }

    private IEnumerator ExecuteModelAsync()
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
        for( int j = 0; j < JointNum; j++ )
        {
            var maxXIndex = 0;
            var maxYIndex = 0;
            var maxZIndex = 0;
            jointPoints[j].score3D = 0.0f;
            var jj = j * HeatMapCol;
            for( int z = 0; z < HeatMapCol; z++ )
            {
                var zz = jj + z;
                for( int y = 0; y < HeatMapCol; y++ )
                {
                    var yy = y * HeatMapCol_Squared * JointNum + zz;
                    for( int x = 0; x < HeatMapCol; x++ )
                    {
                        float v = heatMap3D[yy + x * HeatMapCol_JointNum];
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
           
            jointPoints[j].Now3D.x = (offset3D[maxYIndex * CubeOffsetSquared + maxXIndex * CubeOffsetLinear + j * HeatMapCol + maxZIndex] + 0.5f + (float)maxXIndex) * ImageScale - InputImageSizeHalf;
            jointPoints[j].Now3D.y = InputImageSizeHalf - (offset3D[maxYIndex * CubeOffsetSquared + maxXIndex * CubeOffsetLinear + (j + JointNum) * HeatMapCol + maxZIndex] + 0.5f + (float)maxYIndex) * ImageScale;
            jointPoints[j].Now3D.z = (offset3D[maxYIndex * CubeOffsetSquared + maxXIndex * CubeOffsetLinear + (j + JointNum_Squared) * HeatMapCol + maxZIndex] + 0.5f + (float)(maxZIndex - 14)) * ImageScale;
        }

        // Calculate hip location.
        var lc = (jointPoints[PositionIndex.rThighBend.Int()].Now3D + jointPoints[PositionIndex.lThighBend.Int()].Now3D) / 2f;
        jointPoints[PositionIndex.hip.Int()].Now3D = (jointPoints[PositionIndex.abdomenUpper.Int()].Now3D + lc) / 2f;

        // Calculate neck location.
        jointPoints[PositionIndex.neck.Int()].Now3D = (jointPoints[PositionIndex.rShldrBend.Int()].Now3D + jointPoints[PositionIndex.lShldrBend.Int()].Now3D) / 2f;

        // Calculate head location.
        var cEar = (jointPoints[PositionIndex.rEar.Int()].Now3D + jointPoints[PositionIndex.lEar.Int()].Now3D) / 2f;
        var hv = cEar - jointPoints[PositionIndex.neck.Int()].Now3D;
        var nhv = Vector3.Normalize(hv);
        var nv = jointPoints[PositionIndex.Nose.Int()].Now3D - jointPoints[PositionIndex.neck.Int()].Now3D;
        jointPoints[PositionIndex.head.Int()].Now3D = jointPoints[PositionIndex.neck.Int()].Now3D + nhv * Vector3.Dot(nhv, nv);

        // Calculate spine location.
        jointPoints[PositionIndex.spine.Int()].Now3D = jointPoints[PositionIndex.abdomenUpper.Int()].Now3D;

        // Kalman filter.
        foreach( Avatar.JointPoint jp in jointPoints )
        {
            kalmanFilter(jp);
        }

        // Low pass filter.
        if( useLowPassFilter )
        {
            foreach( Avatar.JointPoint jp in jointPoints )
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

    private void kalmanFilter( Avatar.JointPoint measurement )
    {
        measurementUpdate(measurement);
        measurement.Pos3D.x = measurement.X.x + (measurement.Now3D.x - measurement.X.x) * measurement.K.x;
        measurement.Pos3D.y = measurement.X.y + (measurement.Now3D.y - measurement.X.y) * measurement.K.y;
        measurement.Pos3D.z = measurement.X.z + (measurement.Now3D.z - measurement.X.z) * measurement.K.z;
        measurement.X = measurement.Pos3D;
    }

	private void measurementUpdate( Avatar.JointPoint measurement )
    {
        measurement.K.x = (measurement.P.x + KalmanParamQ) / (measurement.P.x + KalmanParamQ + KalmanParamR);
        measurement.K.y = (measurement.P.y + KalmanParamQ) / (measurement.P.y + KalmanParamQ + KalmanParamR);
        measurement.K.z = (measurement.P.z + KalmanParamQ) / (measurement.P.z + KalmanParamQ + KalmanParamR);
        measurement.P.x = KalmanParamR * (measurement.P.x + KalmanParamQ) / (KalmanParamR + measurement.P.x + KalmanParamQ);
        measurement.P.y = KalmanParamR * (measurement.P.y + KalmanParamQ) / (KalmanParamR + measurement.P.y + KalmanParamQ);
        measurement.P.z = KalmanParamR * (measurement.P.z + KalmanParamQ) / (KalmanParamR + measurement.P.z + KalmanParamQ);
    }
}