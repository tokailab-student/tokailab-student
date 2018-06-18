using UnityEngine;
using System.IO;
public class MainController7 : MonoBehaviour
{
    GameObject[] persons = new GameObject[8];
    Camera[] Cameras = new Camera[8];
    Camera NearCamera;  //視点カメラとなす角が一番小さいカメラ
    Camera targetCamera; //ビルボード処理の対象となる視点カメラ

    int CameraCounter = 8;   //カメラの数
    int CameraNumber = 0;    //カメラの番号

    Vector3 InitialPosition = new Vector3(0, 0, 0);
    Quaternion InitialRotation = new Quaternion();

    //int frame = 0;           //フレーム数
    int frameCounter = 0;    //同期したフレーム数
    int personCounter = 0;   //プレイヤーの人数
    int personNumber = 0;    //プレイヤーの番号
    int Maxframe = 300;       //最大フレーム数
    //int Maxframe = 96;       //最大フレーム数
    float angle;              //視点カメラと固定カメラのなす角
    float MinAngle;           //視点カメラと固定カメラのなす最小角
    string url;               //textuteの読み込み先

    float framespeed = 1f / 30;  //30fps
    int speedControll = 1;    //再生スピード
    float Limit = 0;
    float nowTime = 0;
    bool pauseFlag = false;
    int Offset = 0;

    string text;
    string[] layoutInfo;
    string[] eachInfo;
    Vector3[] pos = new Vector3[8];
    Vector3 target;

    GameObject[,] copyPersons = new GameObject[3, 300];
    int copyNumber = 0;
    int[,] copyFrames = new int[300, 300];
    bool copyFlag = false;
    string copyUrl;
    public Camera[,] NearCameras = new Camera[255, 255];  //視点カメラとなす角が一番小さいカメラ

    Texture[,,] textures = new Texture[300,8,3];

    //string FileName = "TEXTURE_300F";
    string FileName = "TEXTURE_WALK_AROUND";
   

    //最初に一度だけ起動
    void Start()
    {
        //対象のカメラが指定されない場合にはMainCameraを対象とする
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        InitialPosition = Camera.main.transform.position;
        InitialRotation = Camera.main.transform.rotation;

        //Camera位置の取得
        for (int i = 0; i < CameraCounter; i++)
        {
            string text1 = File.ReadAllText(Application.dataPath + "/../Cameras/Camera" + i.ToString() + ".txt");
            string[] layoutInfo1 = text1.Split(',');
            Camera cam = Resources.Load("Camera", typeof(Camera)) as Camera;
            Cameras[i] = Instantiate(cam, new Vector3(float.Parse(layoutInfo1[0]), float.Parse(layoutInfo1[1]), float.Parse(layoutInfo1[2])), new Quaternion()) as Camera;

            Cameras[i].name = "Camera" + i.ToString();

            //カメラを内側に向ける
            Cameras[i].transform.LookAt(new Vector3(0, Cameras[i].transform.position.y, 0));

            //一度カメラを全てOFFにする
            Cameras[i].enabled = false;
        }

        for (int i = 0; i < Maxframe; i++)
        {
            for (int j = 0; j < CameraCounter; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    url = Application.dataPath + "/../" + FileName + "/Frame" + i.ToString() + "/Camera" + j.ToString() + "/texture_c" + j.ToString() + "_p" + k.ToString() + ".png";
                    if (System.IO.File.Exists(url) == true)
                    {
                        WWW Tex = new WWW("file://" + url);  //URL先のtexture読み出し
                        textures[i,j,k] = Tex.texture;  //ビルボードへのtexture貼り付け
                        Tex.Dispose();
                    }
                    else
                    {
                        textures[i, j, k] = Resources.Load("person", typeof(Texture)) as Texture;
                        Debug.Log("not i:" + i.ToString() + ", j: " + j.ToString() + ", k: " +  k.ToString());
                    }             
                }
            }
        }

        NearCamera = Cameras[0];

        Limit = Maxframe * framespeed;

        MakePerson();  //playerの作成やtextureの切り替え     
    }


    //毎フレーム起動
    void Update()
    {
    
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();  //Escを押すとアプリケーションの終了
        }

        //nowTimeがLimitを超えないようにすることでシーンをループさせる
        nowTime = Time.time % Limit;



        //現在の時刻から一番近いフレームを選択(小数点以下切り捨て) ※切り上げるとMaxframeを超える
        frameCounter = ((int)(nowTime / framespeed) + Mathf.Abs(Offset)) % Maxframe;

        Debug.Log("MainController7: FrameCount:" + frameCounter + ", TotalTime: " + Time.time + ", Speed: " + Time.timeScale + "倍速");

        MovePerson();  //playerの移動やtextureの切り替え

        ChangeCamera();  //固定カメラへの視点切り替え

        CheckFlag();

        MakeCopy();

        CheckSpeed();

        Resources.UnloadUnusedAssets();  //現在使用していないアセットを破棄
    }

    void MakePerson()
    {
        text = File.ReadAllText(Application.dataPath + "/../" + FileName + "/Frame" + frameCounter.ToString() + "/person" + frameCounter.ToString() + ".txt");
        layoutInfo = text.Split('\n');
        personCounter = layoutInfo.Length;


        personNumber = 0;
        for (int i = 0; i < personCounter; i++)
        {
            eachInfo = layoutInfo[i].Split(',');
            personNumber = int.Parse(eachInfo[0]);
            pos[i] = new Vector3(float.Parse(eachInfo[1]), float.Parse(eachInfo[2]), float.Parse(eachInfo[3]));

            //Instantiate(persons[personNumber], pos, persons[personNumber].transform.rotation);
            GameObject prefab = Resources.Load("Person", typeof(GameObject)) as GameObject;
            persons[personNumber] = Instantiate(prefab, pos[i], new Quaternion()) as GameObject;
            persons[personNumber].name = "Player" + personNumber.ToString();

            SetCamera();
        }
    }

    void MovePerson()
    {
        text = File.ReadAllText(Application.dataPath + "/../" + FileName + "/Frame" + frameCounter.ToString() + "/person" + frameCounter.ToString() + ".txt");
        layoutInfo = text.Split('\n');
        personCounter = layoutInfo.Length;

        personNumber = 0;
        for (int i = 0; i < personCounter; i++)
        {
            eachInfo = layoutInfo[i].Split(',');
            personNumber = int.Parse(eachInfo[0]);
            pos[i] = new Vector3(float.Parse(eachInfo[1]), float.Parse(eachInfo[2]), float.Parse(eachInfo[3]));

            persons[personNumber].transform.position = pos[i];
            SetCamera();
        }
    }

    void SetCamera()
    {
        url = "file://" + Application.dataPath + "/../" + FileName + "/Frame" + frameCounter.ToString() + "/";

        angle = 0;
        MinAngle = 180;
        NearCamera = Cameras[0];

        for (int i = 0; i < 8; i++)
        {
            //視点カメラと固定カメラのなす角をとる
            angle = Vector3.Angle(Camera.main.transform.position - persons[personNumber].transform.position, Cameras[i].transform.position - persons[personNumber].transform.position);

            if (angle < MinAngle)
            {
                MinAngle = angle;
                NearCamera = Cameras[i];
                targetCamera = NearCamera;
            }
        }


        //Texture切り替え
        for (CameraNumber = 0; CameraNumber < CameraCounter; CameraNumber++)
        {
            if (NearCamera == Cameras[CameraNumber])
            {
                persons[personNumber].transform.FindChild("Billboard").GetComponent<Renderer>().material.mainTexture = textures[frameCounter,CameraNumber,personNumber];  //ビルボードへのtexture貼り付け
            }
        }

        //LookCamera
        Vector3 target = this.targetCamera.transform.position;
        target.y = this.transform.position.y;
        persons[personNumber].transform.LookAt(target);

    }

    void ChangeCamera()  //固定カメラへの視点切り替え
    {
        if (Input.GetKey("0"))  //0番カメラへの移動
        {
            for (int i = 0; i < CameraCounter; i++)
            {
                Cameras[i].enabled = false;
            }
            //Cameras[0].enabled = true;
            targetCamera = Cameras[0];
            Camera.main.transform.position = Cameras[0].transform.position;
            Camera.main.transform.rotation = Cameras[0].transform.rotation;
        }
        else if (Input.GetKey("1"))  //1番カメラへの移動
        {
            for (int i = 0; i < CameraCounter; i++)
            {
                Cameras[i].enabled = false;
            }
            //Cameras[1].enabled = true;
            targetCamera = Cameras[1];
            Camera.main.transform.position = Cameras[1].transform.position;
            Camera.main.transform.rotation = Cameras[1].transform.rotation;
        }
        else if (Input.GetKey("2"))  //2番カメラへの移動
        {
            for (int i = 0; i < CameraCounter; i++)
            {
                Cameras[i].enabled = false;
            }
            //Cameras[2].enabled = true;
            targetCamera = Cameras[2];
            Camera.main.transform.position = Cameras[2].transform.position;
            Camera.main.transform.rotation = Cameras[2].transform.rotation;
        }
        else if (Input.GetKey("3"))  //3番カメラへの移動
        {
            for (int i = 0; i < CameraCounter; i++)
            {
                Cameras[i].enabled = false;
            }
            //Cameras[3].enabled = true;
            targetCamera = Cameras[3];
            Camera.main.transform.position = Cameras[3].transform.position;
            Camera.main.transform.rotation = Cameras[3].transform.rotation;
        }
        else if (Input.GetKey("4"))  //4番カメラへの移動
        {
            for (int i = 0; i < CameraCounter; i++)
            {
                Cameras[i].enabled = false;
            }
            //Cameras[4].enabled = true;
            targetCamera = Cameras[4];
            Camera.main.transform.position = Cameras[4].transform.position;
            Camera.main.transform.rotation = Cameras[4].transform.rotation;
        }
        else if (Input.GetKey("5"))  //5番カメラへの移動
        {
            for (int i = 0; i < CameraCounter; i++)
            {
                Cameras[i].enabled = false;
            }
            //Cameras[5].enabled = true;
            targetCamera = Cameras[5];
            Camera.main.transform.position = Cameras[5].transform.position;
            Camera.main.transform.rotation = Cameras[5].transform.rotation;
        }
        else if (Input.GetKey("6"))  //6番カメラへの移動
        {
            for (int i = 0; i < CameraCounter; i++)
            {
                Cameras[i].enabled = false;
            }
            //Cameras[6].enabled = true;
            targetCamera = Cameras[6];
            Camera.main.transform.position = Cameras[6].transform.position;
            Camera.main.transform.rotation = Cameras[6].transform.rotation;
        }
        else if (Input.GetKey("7"))  //7番カメラへの移動
        {
            for (int i = 0; i < CameraCounter; i++)
            {
                Cameras[i].enabled = false;
            }
            //Cameras[7].enabled = true;
            targetCamera = Cameras[7];
            Camera.main.transform.position = Cameras[7].transform.position;
            Camera.main.transform.rotation = Cameras[7].transform.rotation;
        }
        else if (Input.GetKey("c"))  //カメラ位置のクリア
        {
            for (int i = 0; i < CameraCounter; i++)
            {
                Cameras[i].enabled = false;
            }
            //targetCamera = Camera.main;
            Camera.main.transform.position = InitialPosition;
            Camera.main.transform.rotation = InitialRotation;


            for (int i = 0; i < personCounter; i++)
            {
                for (int j = 0; j < copyNumber; j++)
                {
                    Destroy(copyPersons[i, j]);
                }
            }

            copyNumber = 0;
            copyFlag = false;
        }
    }

    void CheckFlag()
    {
        if (Input.GetKeyDown(KeyCode.P))  //pを押すとframeCounterをストップさせる
        {
            if (pauseFlag == true)
            {
                pauseFlag = false;
                Time.timeScale = 1;
            }
            else
            {
                pauseFlag = true;
                Time.timeScale = 0;
            }
        }
    }

    void MakeCopy()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < personCounter; i++)
            {

                GameObject prefab = Resources.Load("Person", typeof(GameObject)) as GameObject;
                copyPersons[i, copyNumber] = Instantiate(prefab, pos[i], persons[i].transform.rotation) as GameObject;
                //(copyPersons[i, copyNumber].GetComponent("setLife") as MonoBehaviour).enabled = true;
                copyPersons[i, copyNumber].name = "Player" + i.ToString() + "_copy" + copyNumber;
                copyFrames[i, copyNumber] = frameCounter;
                //Debug.Log(copyFrames[i, copyNumber]);
                copyFlag = true;
            }

            copyNumber++;
            copyNumber = copyNumber % Maxframe;
        }

        if (copyFlag == true)
        {
            for (int i = 0; i < personCounter; i++)
            {
                for (int j = 0; j < copyNumber; j++)
                {
                    angle = 0;
                    MinAngle = 180;
                    NearCamera = Cameras[0];

                    for (int k = 0; k < 8; k++)
                    {
                        //視点カメラと固定カメラのなす角をとる
                        angle = Vector3.Angle(Camera.main.transform.position - copyPersons[i, j].transform.position, Cameras[k].transform.position - copyPersons[i, j].transform.position);

                        if (angle < MinAngle)
                        {
                            MinAngle = angle;
                            NearCameras[i, j] = Cameras[k];
                        }
                    }

                    //Texture切り替え
                    for (CameraNumber = 0; CameraNumber < CameraCounter; CameraNumber++)
                    {
                        if (NearCameras[i, j] == Cameras[CameraNumber])
                        {
                            copyPersons[i, j].transform.FindChild("Billboard").GetComponent<Renderer>().material.mainTexture = textures[copyFrames[i, j], CameraNumber, i];  //ビルボードへのtexture貼り付け
                        }
                    }

                    //LookCamera
                    Vector3 target2 = this.NearCameras[i, j].transform.position;
                    target2.y = this.transform.position.y;
                    copyPersons[i, j].transform.LookAt(target2);
                }
            }
        }
    }

    void CheckSpeed()
    {
        //'+'と'-'ボタンでspeedControllの増減を調整
        if (Input.GetKeyDown(KeyCode.KeypadPlus) && speedControll < 3)
        {
            speedControll += 1;
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus) && speedControll > -3)
        {
            speedControll -= 1;
        }

        //speedControllの値によって進行時間の調整
        if (pauseFlag == true)
        {
            Time.timeScale = 0;
        }
        else if (speedControll < 0)
        {
            Time.timeScale = 1f / Mathf.Pow(2f, Mathf.Abs(speedControll));
        }
        else
        {
            Time.timeScale = speedControll;
        }
    }
}
