using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse drag Orbit with zoom")]
public class CameraController : MonoBehaviour {

    float minFov = 15f;
    float maxFov = 90f;
    float sensitivity = 10f;

    float speed = 2.0f;
    float speed2 = 0.5f;
    Transform position;
    private float timeI = 0.0f;
    private Vector3 originPos;
    public float lerpSmooth = 1f;
    public float lerpRSmooth = 2f;

    // Use this for initialization
    void Start () {
        position = GameObject.FindWithTag("Player").transform;
        originPos = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        float fov = Camera.main.fieldOfView;
        fov -= Input.GetAxis("Mouse ScrollWheel") * sensitivity;
        fov = Mathf.Clamp(fov, minFov, maxFov);
        Camera.main.fieldOfView = fov;
                    
        if (Input.GetMouseButton(0))
        {
            transform.RotateAround(position.position, Vector3.up, Input.GetAxis("Mouse X") * speed);
            transform.RotateAround(position.position, Vector3.left, Input.GetAxis("Mouse Y") * speed);
        }
        else
        {
            timeI += 1 * Time.deltaTime;
            if (timeI >= 2)
            {
                originPos = position.TransformPoint(0, 4, -8);
                Vector3 startPos = position.position - transform.position;
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(startPos), lerpRSmooth * Time.deltaTime * 10);
                transform.position = Vector3.Lerp(transform.position, originPos, lerpSmooth * Time.deltaTime * 10);
            }
        }
        transform.LookAt(position);
    }
}
