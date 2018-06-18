using UnityEngine;
using System.IO;

public class MainController6 : MonoBehaviour
{
    GameObject[] persons;   //表示人物のゲームオブジェクト
    Camera[] Cameras;   //各固定カメラ
    Camera NearCamera;  //視点位置に一番近い固定カメラ
    Camera targetCamera; //ビルボードを向ける対象となる固定カメラ

    int MaxPerson = 10;   //最大表示人数(決め打ち)
    int MaxCamera = 0;   //カメラの数(フォルダ数から自動で取得)
    int Maxframe = 0;    //最大フレーム数(ファイル数から自動で取得)

    Vector3 InitialPosition = new Vector3(0, 0, 0);
    Quaternion InitialRotation = new Quaternion();

    int CameraNumber = 0;    //カメラの番号
    int frameCounter = 0;    //同期したフレーム数
    int personCounter = 0;   //プレイヤーの人数
    int personNumber = 0;    //プレイヤーの番号
    

    float angle;              //視点カメラと固定カメラのなす角
    float MinAngle;           //視点カメラと固定カメラのなす最小角
    string url;               //textuteの読み込み先


    float framespeed = 1f / 30;  //30fps
    int speedControll = 1;    //再生スピード
    float Limit = 0;
    float nowTime = 0;
    bool pauseFlag = false; //一時停止機能のフラグ
    int Offset = 0;   //Offsetフレームから表示が開始される
 
    string text;
    string[] layoutInfo;
    string[] eachInfo;
    Vector3[] pos;
    Vector3 target;

    GameObject[,] copyPersons;
    int copyNumber = 0;
    int[,] copyFrames;
    bool copyFlag = false;
    string copyUrl;
    public Camera[,] NearCameras = new Camera[255, 255];  //視点カメラとなす角が一番小さいカメラ

    //FileNameを変更することで再生するシーンを変更する
        //string FileName = "TEXTURE_300F";  // 背景付き
        string FileName = "TEXTURE_WALK_AROUND";  //8cam          円状に歩く
        //string FileName = "TEXTURE_WALK_AROUND_blank"; //8cam
        //string FileName = "TEXTURE_Billboard";     //10cam          ボールをパスする
        //string FileName = "Billboard_WalkThree";   //10cam　　　　三人が交差して歩く

    //FileNameを変更することでカメラ台数を変更する。FileNameとの対応についてはREADME.txt参照。
        //string CameraFile = "CameraPosition"; //10cam
        string CameraFile = "Cameras";          //8cam

    //void Start(){}内は最初に一度だけ呼び出される
    void Start()
    {
        //対象のカメラが指定されない場合にはMainCameraを対象とする
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        //クリアできるようメインカメラの初期位置を保存
        InitialPosition = Camera.main.transform.position;
        InitialRotation = Camera.main.transform.rotation;

        //最大フレーム数を計算
        url = Application.dataPath + "/../" + FileName + "/Frame0";
        for (Maxframe = 0; System.IO.Directory.Exists(url) == true; )
        {
            Maxframe++;
            url = Application.dataPath + "/../" + FileName + "/Frame" + Maxframe.ToString();    
        }

        //最大カメラ数を計算
        url = Application.dataPath + "/../" + CameraFile + "/Camera0.txt";
        for (MaxCamera = 0; System.IO.File.Exists(url) == true; )
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

        //Camera位置の取得と配置
        for (int i = 0; i < MaxCamera; i++)
        {
            string text1 = File.ReadAllText(Application.dataPath + "/../" + CameraFile +"/Camera" + i.ToString() + ".txt");

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

        Limit = Maxframe * framespeed;  //最大フレーム数とフレームレートから最大表示時間を設定

        MakePerson();  //ビルボードの生成&移動や貼り付けるテクスチャの切り替え   
    }


    //void Update(){}内は毎フレーム呼び出される
    void Update()
    {
        //Debug.Log("FrameCount:" + frameCounter + ", nowTime:" + nowTime); //コンソールへの表示 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();  //Escを押すとアプリケーションの終了
        }

        CheckFlag(); //一時停止機能

        CheckSpeed();  //再生スピードの調整

        //nowTimeがLimitを超えないようにすることでシーンをループさせる
        nowTime = Time.time % Limit;

        //現在の時刻から一番近いフレームを選択(小数点以下切り捨て) ※切り上げるとMaxframeを超える
        frameCounter = ((int)(nowTime / framespeed) + Mathf.Abs(Offset)) % Maxframe;

        MakePerson();  //ビルボードの生成&移動や貼り付けるテクスチャの切り替え

        //LogSave(frameCounter, Time.time); //性能評価時にcsvファイルでログを保存するのに使用

        ChangeCamera();  //固定カメラへの視点切り替え
       
        MakeCopy(); //複製機能

        Resources.UnloadUnusedAssets();  //現在使用していないアセットを破棄(これをしないとメモリを消費し続ける)
        
    }

    //ビルボードの生成や移動
    void MakePerson()
    {
        //3次元位置の読み込み
        text = File.ReadAllText(Application.dataPath + "/../" + FileName + "/Frame" + frameCounter.ToString() + "/person" + frameCounter.ToString() + ".txt");
        layoutInfo = text.Split('\n');

        //表示人数のカウント(EOFのみの空行はカウントしないようにする)
        int diff = 0;
        for (personCounter = 0;  personCounter < layoutInfo.Length; personCounter++)
        {
            if (layoutInfo[personCounter] == "") //空行をカウント
            {
                diff++;
            }
        }
        personCounter = personCounter - diff;//空行の数だけ引く

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

        //画面上に映らないプレーヤーを消す
        for (int i = personCounter; i < MaxPerson; i++)
        {
            Destroy(persons[i]);
        }
    }

    //一番近い撮影カメラの選択
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

        //Texture切り替え処理
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

        //ビルボードを一番近い撮影カメラ方向に向ける
        Vector3 target = this.targetCamera.transform.position;
        target.y = persons[personNumber].transform.position.y;
        persons[personNumber].transform.LookAt(target);
    }

    //固定カメラへの視点切り替え
    void ChangeCamera()  
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

    //一時停止機能
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

    //複製機能
    void MakeCopy()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < personCounter; i++)
            {
                GameObject prefab = Resources.Load("Person", typeof(GameObject)) as GameObject;
                copyPersons[i, copyNumber] = Instantiate(prefab, pos[i], persons[i].transform.rotation) as GameObject;
                //(copyPersons[i, copyNumber].GetComponent("setLife") as MonoBehaviour).enabled = true; //これを使用するとsetLifeスクリプトがONになり複製ビルボードが時間経過で消える。
                copyPersons[i, copyNumber].name = "Player" + i.ToString() + "_copy" + copyNumber.ToString();
                copyFrames[i, copyNumber] = frameCounter;
              
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

    //再生スピードの調整
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
            Time.timeScale = 0; //経過時間の停止
        }
        else if (speedControll < 0)
        {
            //speedControllが-1なら0.5倍速,-2なら0.25倍速というようにスピードが2分の1になっていく
            Time.timeScale = 1f / Mathf.Pow(2f, Mathf.Abs(speedControll)); 
        }
        else
        {
            //speedControll > 0ならspeedControll倍速になる
            Time.timeScale = speedControll;
        }
    }

    //GUIの表示
    private int value = 0;
    void OnGUI()
    {
        if (Time.timeScale != 0)
        {
            value = frameCounter;
        }
        else
        {
            frameCounter = value;
        }

        //方法2では常に時間の制約を受けるためスクロールバーを使った表示フレームの変更ができない。
        Rect rect1 = new Rect(20, (float)Screen.height - 60, (float)Screen.width - 40, 30);
        value = (int)GUI.HorizontalScrollbar(rect1, value, 1, 0, Maxframe);

        Rect rect2 = new Rect(20, (float)Screen.height - 90, (float)Screen.width - 40, 30);
        GUI.Label(rect2, "Camera = " + targetCamera.ToString());

        Rect rect3 = new Rect(20, (float)Screen.height - 120, (float)Screen.width - 40, 30);
        GUI.Label(rect3, "Frame = " + value + " / " + (Maxframe - 1) + " : " + Time.timeScale + "倍速");

        Rect rect4 = new Rect(20, (float)Screen.height - 150, (float)Screen.width - 40, 30);
        GUI.Label(rect4, "FileName = " + FileName);
    }

    //性能評価時にcsvファイルでログを保存するのに使用
    private int LogCounter= 0;//LogSaveが呼ばれた回数をカウントすることで実時間性重視の表示性能を確認する
    public void LogSave(float frame, float totaltime)
    {
        LogCounter++;
        StreamWriter sw;
        FileInfo fi;
        fi = new FileInfo(Application.dataPath + "/../csv/time2.csv");
        sw = fi.AppendText();
        sw.WriteLine(LogCounter + ", " + frame + ", " + totaltime);
        //sw.Flush();
        sw.Close();     
    }
}
