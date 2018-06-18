#pragma strict

var life = 3; //[sec]
var rend : Renderer;

function Start () {
    rend = transform.FindChild("Billboard").GetComponent.<Renderer>();
    Destroy(gameObject, life);
}

function Update () {
    // 透明度の更新
    rend.material.color.a -= Time.deltaTime / life;
}