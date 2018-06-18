using UnityEngine;

public class MoveCamera : MonoBehaviour {


    public float xspeed = 100;
    public float yspeed = 100;
    private float sx;
    private float sy;
    private float dx;
    private float dy;
    private float tx;
    private float ty;
   
    public Camera Cam;

    //マイフレーム更新時に呼び出す
    void Update()
    {

        //十字キーでカメラ移動
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 p = GetComponent<Transform>().position;
        p.x += (float)(0.1 * x);
        p.z += (float)(0.1 * z);
        GetComponent<Transform>().position = p;

        //左クリックでy軸‐90度回転
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 q = GetComponent<Transform>().position;
            q.x = -p.z;
            q.z = p.x;
            transform.Rotate(0, -90, 0);
            GetComponent<Transform>().position = q;
        }
        //GetComponent<Transform>().position = new Vector3(GetComponent<Transform>().position.x + (float)(0.1 * x), GetComponent<Transform>().position.y, GetComponent<Transform>().position.z + (float)(0.1 * z));

        if (Input.GetMouseButtonDown(1))
        {
            //Input Mouse position
            sx = Input.mousePosition.x;
            sy = Input.mousePosition.y;
        }
        //Drag
        if (Input.GetMouseButton(1))
        {
            dx = Input.mousePosition.x;
            dy = Input.mousePosition.y;
            tx = sx - dx;
            ty = sy - dy;
            Cam.transform.eulerAngles += new Vector3(-ty / yspeed * Time.deltaTime * 15, tx / xspeed * Time.deltaTime * 15, 0);
        }

    }

}