using UnityEngine;
using System.Collections;

public class Gizmo : MonoBehaviour {

    void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(transform.position, 1.0f);

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 direction = transform.TransformDirection(Vector3.forward) * 3;
        Gizmos.DrawRay(transform.position, direction);
        //Gizmos.DrawLine(transform.position + direction, new Vector3(0,0,-1));


    }

}
