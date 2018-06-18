using UnityEngine;
using System.IO;

public class MainController9 : MonoBehaviour
{
    GameObject[] persons;// = new GameObject[8];
    Camera[] Cameras;// = new Camera[10];
    Camera NearCamera;  //視点カメラとなす角が一番小さいカメラ
    Camera targetCamera; //ビルボード処理の対象となる視点カメラ

    int MaxPerson = 10;
    int MaxCamera = 0;   //カメラの数
    int Maxframe = 0;       //最大フレーム数

    Vector3 InitialPosition = new Vector3(0, 0, 0);
    Quaternion InitialRotation = new Quaternion();

    int CameraCounter = 8;   //カメラの数
    int CameraNumber = 0;    //カメラの番号
    //int frame = 0;           //フレーム数
    int frameCounter = 0;    //同期したフレーム数
    int personCounter = 0;   //プレイヤーの人数
    int personNumber = 0;    //プレイヤーの番号

    float angle;              //視点カメラと固定カメラのなす角
    float MinAngle;           //視点カメラと固定カメラのなす最小角
    string url;               //textuteの読み込み先
    bool pauseFlag = false;

    float timeFrame = 0;
    float framespeed = 1f / 30;  //30fps
    int speedControll = 1;    //再生スピード
    int Offset = 0;  //開始フレーム

    string text;
    string[] layoutInfo;
    string[] eachInfo;
    Vector3[] pos;// = new Vector3[8];
    Vector3 target;

    GameObject[,] copyPersons;// = new GameObject[3, 300];
    int copyNumber = 0;
    int[,] copyFrames;// = new int[300, 300];
    bool copyFlag = false;
    string copyUrl;
    public Camera[,] NearCameras = new Camera[255, 255];  //視点カメラとなす角が一番小さいカメラ

    //string FileName = "TEXTURE_300F";
    string FileName = "TEXTURE_WALK_AROUND";
    //string FileName = "TEXTURE_WALK_AROUND_blank";
    //string FileName = "TEXTURE_Billboard";
    //string FileName = "Billboard_WalkThree";


    //string CameraFile = "CameraPosition";
    string CameraFile = "Cameras";


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

        //最大フレーム数を計算
        url = Application.dataPath + "/../" + FileName + "/Frame0";
        for (Maxframe = 0; System.IO.Directory.Exists(url) == true;)
        {
            Maxframe++;
            url = Application.dataPath + "/../" + FileName + "/Frame" + Maxframe.ToString();
        }

        //最大カメラ数を計算
        url = Application.dataPath + "/../" + CameraFile + "/Camera0.txt";
        for (MaxCamera = 0; System.IO.File.Exists(url) == true;)
        {
            MaxCamera++;
            url = Application.dataPath + "/../" + CameraFile + "/Camera" + MaxCamera.ToString() + ".txt";
        }

        //使用する配列の初期化
        persons = new GameObject[MaxPerson];
        Cameras = new Camera[MaxCamera];
        pos = new Vector3[MaxPerson];
        copyPersons = new GameObject[MaxPerson, Maxframe];
        copyFrames = new int[Maxframe, Maxframe];

        //Camera位置の取得
        for (int i = 0; i < MaxCamera; i++)
        {
            //string text1 = File.ReadAllText(Application.dataPath + "/../CameraPosition/Camera" + i.ToString() + ".txt");
            string text1 = File.ReadAllText(Application.dataPath + "/../" + CameraFile + "/Camera" + i.ToString() + ".txt");

            string[] layoutInfo1 = text1.Split(',');
            Camera cam = Resources.Load("Camera", typeof(Camera)) as Camera;
            Cameras[i] = Instantiate(cam, new Vector3(float.Parse(layoutInfo1[0]), float.Parse(layoutInfo1[1]), float.Parse(layoutInfo1[2])), new Quaternion()) as Camera;

            Cameras[i].name = "Camera" + i.ToString();

            //カメラを内側に向ける
            Cameras[i].transform.LookAt(new Vector3(0, Cameras[i].transform.position.y, 0));

            //カメラを全てOFFにする
            Cameras[i].enabled = false;
        }

        NearCamera = Cameras[0];

        frameCounter = Mathf.Abs(Offset) % Maxframe;
        //Application.targetFrameRate = 30;

        MakePerson();  //playerの作成やtextureの切り替え
    }


    //毎フレーム起動
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();  //Escを押すとアプリケーションの終了
        }

        timeFrame += Time.deltaTime;

        CheckFlag();  //pauseFlagのチェック

        if (Input.GetKeyDown(KeyCode.KeypadPlus) && speedControll < 10)
        {
            speedControll += 1;
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus) && speedControll > -10)
        {
            speedControll -= 1;
        }


        //フレーム間の時間がframespeedを超えたら表示フレームを更新する(同期処理)
        if (timeFrame >= framespeed)
        {
            //logSave(frameCounter,timeFrame, Time.time);

            MakePerson();  //playerの移動やtextureの切り替え
            //Debug.Log("MainController9: FrameCount:" + frameCounter + ", Player0:" + persons[0].transform.position + ", TargetCamera:" + targetCamera.ToString() + ", 再生速度:" + speedControll + "倍速"); //コンソールへの表示


            if (speedControll == 0 || pauseFlag == true)  //speed'p'を押すとframeCounterを止めることで停止する
            {
                Debug.Log("FrameCount:" + frameCounter + ", Player0:" + persons[0].transform.position + ", TargetCamera:" + targetCamera.ToString() + ", 再生速度:Pause"); //コンソールへの表示
            }
            else if (speedControll < 0)  //speedControllがマイナスのときは逆再生する
            {
                if (frameCounter < Mathf.Abs(speedControll))  //frameCounterがマイナスにならないようMaxframeを加える
                {
                    frameCounter += Maxframe;
                }
                frameCounter += speedControll;
            }
            else  //何も押さなければframeCounterをspeedControll分進める
            {
                frameCounter += speedControll;
                frameCounter = frameCounter % Maxframe;  //300フレーム毎に0フレーム目に戻る（シーンのループ）
            }

            ChangeCamera();  //固定カメラへの視点切り替え

            timeFrame -= framespeed;
        }

        MakeCopy();
        Resources.UnloadUnusedAssets();  //現在使用していないアセットを破棄
    }

    void MakePerson()
    {
        text = File.ReadAllText(Application.dataPath + "/../" + FileName + "/Frame" + frameCounter.ToString() + "/person" + frameCounter.ToString() + ".txt");
        layoutInfo = text.Split('\n');

        //表示人数のカウント(EOFのみの行はカウントしない)
        int diff = 0;
        for (personCounter = 0; personCounter < layoutInfo.Length; personCounter++)
        {
            if (layoutInfo[personCounter] == "")
            {
                diff++;
            }
        }
        personCounter = personCounter - diff;

        //personCounter = layoutInfo.Length - 1;

        personNumber = 0;
        for (int i = 0; i < personCounter; i++)
        {
            eachInfo = layoutInfo[i].Split(',');
            personNumber = int.Parse(eachInfo[0]);
            pos[i] = new Vector3(float.Parse(eachInfo[1]), float.Parse(eachInfo[2]), float.Parse(eachInfo[3]));

            if (GameObject.Find("Player" + personNumber.ToString()) != null)
            {
                //プレーヤーが存在すれば移動のみを行う
                persons[personNumber].transform.position = pos[i];
            }
            else
            {
                //プレーヤーが存在しなければ生成する
                GameObject prefab = Resources.Load("Person", typeof(GameObject)) as GameObject;
                persons[personNumber] = Instantiate(prefab, pos[i], new Quaternion()) as GameObject;
                persons[personNumber].name = "Player" + personNumber.ToString();
            }
            SetCamera();
        }

        //画面上に移らないプレーヤーを消す
        for (int i = personCounter; i < MaxPerson; i++)
        {
            Destroy(persons[i]);
        }
    }

    void SetCamera()
    {
        url = Application.dataPath + "/../" + FileName + "/Frame" + frameCounter.ToString() + "/";

        angle = 0;
        MinAngle = 180;
        NearCamera = Cameras[0];

        for (int i = 0; i < MaxCamera; i++)
        {
            //視点カメラと固定カメラのなす角をとる
            angle = Vector3.Angle(Camera.main.transform.position - persons[personNumber].transform.position, Cameras[i].transform.position - persons[personNumber].transform.position);

            if (angle < MinAngle && System.IO.File.Exists(Application.dataPath + "/../" + FileName + "/Frame" + frameCounter.ToString() + "/Camera" + i.ToString() + "/texture_c" + i.ToString() + "_p" + personNumber.ToString() + ".png") == true)
            {
                MinAngle = angle;
                NearCamera = Cameras[i];
                targetCamera = NearCamera;
            }
        }
        //Debug.Log("ViewCamera:::" + NearCamera);

        //Texture切り替え
        for (CameraNumber = 0; CameraNumber < MaxCamera; CameraNumber++)
        {
            if (NearCamera == Cameras[CameraNumber])
            {
                url = Application.dataPath + "/../" + FileName + "/Frame" + frameCounter.ToString() + "/Camera" + CameraNumber.ToString() + "/texture_c" + CameraNumber.ToString() + "_p" + personNumber.ToString() + ".png";

                WWW Tex = new WWW("file://" + url);  //URL先のtexture読み出し
                persons[personNumber].transform.FindChild("Billboard").GetComponent<Renderer>().material.mainTexture = Tex.texture;  //ビルボードへのtexture貼り付け
                Tex.Dispose();
            }
        }

        //LookCamera
        Vector3 target = this.targetCamera.transform.position;
        target.y = persons[personNumber].transform.position.y;
        persons[personNumber].transform.LookAt(target);

    }



    void ChangeCamera()  //固定カメラへの視点切り替え
    {

        if (Input.GetKey("0"))  //0番カメラへの移動
        {
            targetCamera = Cameras[0];
            Camera.main.transform.position = Cameras[0].transform.position;
            Camera.main.transform.rotation = Cameras[0].transform.rotation;
        }
        else if (Input.GetKey("1"))  //1番カメラへの移動
        {
            targetCamera = Cameras[1];
            Camera.main.transform.position = Cameras[1].transform.position;
            Camera.main.transform.rotation = Cameras[1].transform.rotation;
        }
        else if (Input.GetKey("2"))  //2番カメラへの移動
        {
            targetCamera = Cameras[2];
            Camera.main.transform.position = Cameras[2].transform.position;
            Camera.main.transform.rotation = Cameras[2].transform.rotation;
        }
        else if (Input.GetKey("3"))  //3番カメラへの移動
        {
            targetCamera = Cameras[3];
            Camera.main.transform.position = Cameras[3].transform.position;
            Camera.main.transform.rotation = Cameras[3].transform.rotation;
        }
        else if (Input.GetKey("4"))  //4番カメラへの移動
        {
            targetCamera = Cameras[4];
            Camera.main.transform.position = Cameras[4].transform.position;
            Camera.main.transform.rotation = Cameras[4].transform.rotation;
        }
        else if (Input.GetKey("5"))  //5番カメラへの移動
        {
            targetCamera = Cameras[5];
            Camera.main.transform.position = Cameras[5].transform.position;
            Camera.main.transform.rotation = Cameras[5].transform.rotation;
        }
        else if (Input.GetKey("6"))  //6番カメラへの移動
        {
            targetCamera = Cameras[6];
            Camera.main.transform.position = Cameras[6].transform.position;
            Camera.main.transform.rotation = Cameras[6].transform.rotation;
        }
        else if (Input.GetKey("7"))  //7番カメラへの移動
        {
            targetCamera = Cameras[7];
            Camera.main.transform.position = Cameras[7].transform.position;
            Camera.main.transform.rotation = Cameras[7].transform.rotation;
        }
        else if (Input.GetKey("8"))  //8番カメラへの移動
        {
            if (GameObject.Find("Camera8") != null)
            {
                targetCamera = Cameras[8];
                Camera.main.transform.position = Cameras[8].transform.position;
                Camera.main.transform.rotation = Cameras[8].transform.rotation;
            }
        }
        else if (Input.GetKey("9"))  //9番カメラへの移動
        {
            if (GameObject.Find("Camera9") != null)
            {
                targetCamera = Cameras[9];
                Camera.main.transform.position = Cameras[9].transform.position;
                Camera.main.transform.rotation = Cameras[9].transform.rotation;
            }
        }
        else if (Input.GetKey("c"))  //カメラ位置のクリア
        {
            Camera.main.transform.position = InitialPosition;
            Camera.main.transform.rotation = InitialRotation;


            for (int i = 0; i < MaxPerson; i++)
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
            }
            else
            {
                pauseFlag = true;
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
                copyPersons[i, copyNumber].name = "Player" + i.ToString() + "_copy" + copyNumber.ToString();
                copyFrames[i, copyNumber] = frameCounter;
                //Debug.Log(copyFrames[i, copyNumber]);
                copyFlag = true;
            }
            copyNumber++;
            copyNumber = copyNumber % Maxframe;
        }

        if (copyFlag == true)
        {
            for (int i = 0; i < MaxPerson; i++)
            {
                for (int j = 0; j < copyNumber; j++)
                {
                    if (GameObject.Find("Player" + i.ToString() + "_copy" + j.ToString()) != null)
                    {

                        copyUrl = Application.dataPath + "/../" + FileName + "/Frame" + copyFrames[i, j].ToString() + "/";

                        angle = 0;
                        MinAngle = 180;
                        NearCamera = Cameras[0];

                        for (int k = 0; k < MaxCamera; k++)
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
                        for (CameraNumber = 0; CameraNumber < MaxCamera; CameraNumber++)
                        {
                            if (NearCameras[i, j] == Cameras[CameraNumber])
                            {
                                copyUrl += "Camera" + CameraNumber.ToString() + "/texture_c" + CameraNumber.ToString() + "_p" + i.ToString() + ".png";

                                if (System.IO.File.Exists(copyUrl) == true)
                                {
                                    WWW Tex = new WWW("file://" + copyUrl);  //URL先のtexture読み出し
                                    copyPersons[i, j].transform.FindChild("Billboard").GetComponent<Renderer>().material.mainTexture = Tex.texture;  //ビルボードへのtexture貼り付け
                                    Tex.Dispose();
                                }
                                else
                                {
                                    copyPersons[i, j].transform.FindChild("Billboard").GetComponent<Renderer>().material.mainTexture = Resources.Load("person", typeof(Texture)) as Texture;  //ビルボードへのtexture貼り付け
                                }
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
    }

    //GUIの表示
    void OnGUI()
    {   
        //スクロールバーで表示フレームの調節ができる。(方法1の場合のみ)
        Rect rect1 = new Rect(20, (float)Screen.height - 60, (float)Screen.width - 40, 30);
        frameCounter = (int)GUI.HorizontalScrollbar(rect1, frameCounter, 1, 0, Maxframe);

        Rect rect2 = new Rect(20, (float)Screen.height - 90, (float)Screen.width - 40, 30);
        GUI.Label(rect2, "Camera = " + targetCamera.ToString());

        Rect rect3 = new Rect(20, (float)Screen.height - 120, (float)Screen.width - 40, 30);
        GUI.Label(rect3, "Frame = " + frameCounter + " / " + (Maxframe - 1) + " : " + speedControll + "倍速");

        Rect rect4 = new Rect(20, (float)Screen.height - 150, (float)Screen.width - 40, 30);
        GUI.Label(rect4, "FileName = " + FileName);
    }

    public void logSave(float frame, float time, float totaltime)
    {
        StreamWriter sw;
        FileInfo fi;
        fi = new FileInfo(Application.dataPath + "/../csv/time5-1.csv");
        sw = fi.AppendText();
        sw.WriteLine(frame + ": " + time + ", " + totaltime);
        //sw.Flush();
        sw.Close();
    }

}
