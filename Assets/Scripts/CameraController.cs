using UnityEngine;

public class CameraController : MonoBehaviour {

    Vector3 CameraOffset;   // 摄像机偏移位置
    Vector3 CameraOrigin;   // 摄像机初始位置

    // 摄像机归位
    public void BackToOrigin() {
        transform.position = CameraOrigin;
    }

    // 设置偏移位置
    public void MoveByOffset(Vector3 position) {
        CameraOffset.x = position.x;
        CameraOffset.z = position.z;
        CameraOffset.y = 0;
    }

	void Start () {
        CameraOrigin = transform.position;
	}
	
	void Update () {
        transform.position = Vector3.Lerp(transform.position, CameraOffset + CameraOrigin, 0.1f);
	}
}