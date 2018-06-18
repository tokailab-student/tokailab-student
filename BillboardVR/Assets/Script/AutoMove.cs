using UnityEngine;

public class AutoMove : MonoBehaviour
{
    public bool AutoFlag = true;
    public float rad = 0;
    public float r = 10.0f;  //半径r
    
    void Start()
    {
        Vector3 p = GetComponent<Transform>().position;
    }
    //マイフレーム更新時に呼び出す
    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.M)) || (Input.GetKeyDown(KeyCode.C)))
        {
            if (AutoFlag == false)
            {
                AutoFlag = true;
                //キーボード・マウス操作を無効にする。
                (Camera.main.GetComponent("cameraControlWithMouse") as MonoBehaviour).enabled = false;
                (Camera.main.GetComponent("cameraControlWithKey") as MonoBehaviour).enabled = false;
            }
            else
            {
                AutoFlag = false;
                //キーボード・マウス操作を有効にする。
                (Camera.main.GetComponent("cameraControlWithMouse") as MonoBehaviour).enabled = true;
                (Camera.main.GetComponent("cameraControlWithKey") as MonoBehaviour).enabled = true;
            }     
        }

        if (AutoFlag == true){

            //半径rの円運動をする
            if (rad < 2 * Mathf.PI)
            {
                Vector3 p = GetComponent<Transform>().position;
                p.x = Mathf.Cos(rad) * r;
                p.y =  1;
                p.z = Mathf.Sin(rad) * r;
                GetComponent<Transform>().position = p;
                rad += 0.001f;
            }
            else
            {
                rad = 0;
            }

            //カメラを中央に向ける
            Vector3 target = new Vector3(0, 1, 0);
            GetComponent<Transform>().transform.LookAt(target);
        }
    }
}
