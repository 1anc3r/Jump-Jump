using UnityEngine;

public class CameraController : MonoBehaviour {

    Vector3 CameraOffset;
    Vector3 CameraOrigin;

    public void BackToOrigin() {
        transform.position = CameraOrigin;
    }

    public void MoveByOffset(Vector3 position) {
        CameraOffset.x = position.x;
        CameraOffset.z = position.z;
        CameraOffset.y = 0;
    }

	// Use this for initialization
	void Start () {
        CameraOrigin = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.Lerp(transform.position, CameraOffset + CameraOrigin, 0.1f);
	}
}