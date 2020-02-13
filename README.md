# ThreeDPoseUnityBarracuda
Unity sample of 3D pose estimation using Barracuda

# Now in preparation

## Outline
ThreeDPoseUnityBarracuda is a sample source which read the onnx by Barracuda and do threeD pose estimation on Unity.
the accuracy got better than pre model.

This sample let the avatar named as "Unity chan" behaves same as the man on real time by estimating the 3D positions on the movie.</br>
![preview_capture_v2.gif](preview_capture_v2.gif)</br></br>
We use Barracuda 0.4 to load onnx.</br>
By updating the version to 0.5, the speed seemed to get slower. </br>
Now we are still trying 0.5 version. </br>
</br>
## Performance
### GPU </br>
GeForce RTX2060 SUPER ⇒ 30 FPS </br>
GeForce GTX1070 ⇒ 20 FPS </br>

## Install and Tutorial
### Download and put files
1. ~~Put the folders named ad "Assets" and "Packages" in your Unity Project.~~
   Now we have added project settings to the code. So please just download/clone them to your local PC.

2. Download onnx from our home page by clicking following URL in our HP.</br>
   http://digital-standard.com/threedpose/models/Resnet34_3inputs_448x448_20200212.onnx
   
### Settings in Unity Inspector
1. Open the Unity project with Unity Editor and put the onnx file in /Assets/Scripts/Model/ 
   In this action, the onnx file is being converted into NNModel type of Barracuda automatically.

2. Open "SampleScene" in "Scene" folder.</br>
   If dialog shows up, please choose "Don't save".</br></br>
3. Set model</br>
   Drag the NNModel you put before in Assets/Scripts/Model/ 
   and drop it to "NN Model" in Gameobject named as "BarracudaRunner" in Inspector view.
   ![unity_inspector.PNG](unity_inspector.PNG)
   
4. Start Debug</br>
   Now you can see real time motion capture by starting Debug.
   ![unity_debug..PNG](unity_debug..PNG)
   
## ※Option<br>
・ Choose Video</br>
   You can choose the target video.</br>
   Put the video you choose in Assets/Video/, and then drag the file and drop it to Video Clip of "Video Player".<br>
   ![unity_chooseVideo.PNG](unity_chooseVideo.PNG)
   
・Choose Avatar</br>
    There are two types of avatar in this Scene.</br>
    You can change the avatar easily in inspector view.</br>
    Firstly activate Gameobject named as "Tait" and deactivate "unitychan".</br>
    Secondly drag the Gameobject and drop it to "V Nect Model" of "BarracudaRunner".</br>
    ![unity_set_anoter_avater_to_obj.PNG](unity_set_anoter_avater_to_obj.PNG)</br>
    
    *To determin direction of the face of avatar, a gameobject which works as nose has been added in those of avatars.
     So if you would like to adopt your original avatar, please add the nose referencing the code.
    
・Set size of video screen</br>
   The size of video screen affect the size of avatar.</br>
   You can set the number in "Video Background Scale" of "MainTexture". The range is 0.1 ~ 1. Default value is 1.<br>
   ![unity_set_size_of_video.PNG](unity_set_size_of_video.PNG)</br>
   
・Use Web Camera
   By checking "Use Web Cam", you can change the input images.</br>
   ![unity_use_web_cam.PNG](unity_use_web_cam.PNG)</br>
   
・Skip On Drop</br>
   If "Skip On Drop" in Video Player checked, VideoPlayer is allowed to skip frames to catch up with current time.<br>
   
## Info
・Record</br>
If you want to record the motion, the following package might be suitable.</br>
https://github.com/zizai-inc/EasyMotionRecorder</br>

## License
Non-commercial use only.Please use it freely for hobbies and research. When redistributing, it would be appreciated if you could enter a credit (Digital-Standard Co., Ltd.).Please contact us if you want to use it for commercial purposes.

   


   
