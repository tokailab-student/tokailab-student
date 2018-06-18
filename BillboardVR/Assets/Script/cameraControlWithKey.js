#pragma strict

var moveSize = 0.1;
var rotateSize = 1;
var initialPosition = Vector3(0, 1, -7);
var initialEulerAngle = Vector3(0, 0, 0);

function getKey(){
    //used on moveTo()
    if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)){
        return "go_forward";
    }
    if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)){
        return "go_back";
    }
    if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)){
        return "go_right";
    }
    if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)){
        return "go_left";
    }
    if(Input.GetKey(KeyCode.PageUp)){
        return "go_up";
    }
    if(Input.GetKey(KeyCode.PageDown)){
        return "go_down";
    }

    //used on rotateAround()
    if(Input.GetKey(KeyCode.E)){
        return "rotate_vertical_positive";
    }
    if(Input.GetKey(KeyCode.Q)){
        return "rotate_vertical_negative";
    }
    if(Input.GetKey(KeyCode.I)){
        return "rotate_horizontal_positive";
    }
    if(Input.GetKey(KeyCode.K)){
        return "rotate_horizontal_negative";
    }
    if(Input.GetKey(KeyCode.Alpha1)){
        return "reset_local_rotation";
    }

    /*
    //used on moveTo() and rotateAround()
    if(Input.GetKey(KeyCode.R)){
        return "reset";
    }*/

    return null;
}

function moveTo(d){ //d = direction
    if(d == "go_forward"){
        //transform.position += Vector3(moveSize, 0, 0);
        transform.Translate(Vector3.forward * moveSize);
    }
    if(d == "go_back"){
        //transform.position += Vector3(-moveSize, 0, 0);
        transform.Translate(Vector3.back * moveSize);
    }
    if(d == "go_right"){
        //transform.position += Vector3(0, 0, -moveSize);
        transform.Translate(Vector3.right * moveSize);
    }
    if(d == "go_left"){
        //transform.position += Vector3(0, 0, moveSize);
        transform.Translate(Vector3.left * moveSize);
    }
    if(d == "go_up"){
        transform.Translate(Vector3.up * moveSize * 0.5, Space.World);
    }
    if(d == "go_down"){
        transform.Translate(Vector3.down * moveSize * 0.5, Space.World);
    }

    if(d == "reset"){
    	transform.position = initialPosition;
    }
}

function rotateAround(a){ //a = axis
    if(a == "rotate_vertical_positive"){
        transform.Rotate(Vector3.up, rotateSize, Space.World);
    }
    if(a == "rotate_vertical_negative"){
        transform.Rotate(Vector3.up, -rotateSize, Space.World);
    }
    if(a == "rotate_horizontal_positive"){
        transform.Rotate(Vector3.right, rotateSize);
    }
    if(a == "rotate_horizontal_negative"){
        transform.Rotate(Vector3.right, -rotateSize);
    }
    if(a == "reset_local_rotation"){
    }

    if(a == "reset"){
    	transform.eulerAngles= initialEulerAngle;
    }
}

function Start () {
}

function Update () {
    var key = getKey();
    moveTo(key);
    rotateAround(key);
}
