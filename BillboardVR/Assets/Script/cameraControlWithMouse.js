#pragma strict

public var moveSpeed = 1.0;
public var rotateSpeed = 5.0;
public var wheelSpeed = 250.0;
private var latestMousePosition : Vector3;

function Start () {
}

function Update () {
    // Mouse Button Action
    if(Input.GetMouseButtonDown(0)
        || Input.GetMouseButtonDown(1)
        || Input.GetMouseButtonDown(2)){
            latestMousePosition = Input.mousePosition;
            //Debug.Log(latestMousePosition);
    } 
    MouseDrag(Input.mousePosition);

    // Mouse Scroll Action
    var scrollWheel = Input.GetAxis("Mouse ScrollWheel");
    if(scrollWheel != 0.0){
         MouseWheel(scrollWheel);
    }
}

function MouseDrag(mousePosition : Vector3){
    var diff : Vector3;
    diff = mousePosition - latestMousePosition;

    // Translation Mode
    if(Input.GetMouseButton(0)){
        transform.Translate(-diff * Time.deltaTime * moveSpeed);
    }

    // Rotation Mode
    if(Input.GetMouseButton(1)){
        //transform.Rotate(Vector3.up, rotateSize, Space.World);
        transform.Rotate(Vector3.up, -diff.x * Time.deltaTime * rotateSpeed, Space.World);
        transform.Rotate(Vector3.right, diff.y * Time.deltaTime * rotateSpeed, Space.Self);
    }

    latestMousePosition = mousePosition;
}

function MouseWheel(delta : float){
    transform.Translate(Vector3.forward * delta * Time.deltaTime * wheelSpeed, Space.Self);
}