using UnityEngine;

public class DebugMover : MonoBehaviour
{
    [SerializeField]
    Transform Head = null;
    public float Angle = 30f;
    public float DashSpeed = 5f;
    public float SlowSpeed = 0.2f;
	// 前進速度
	public float forwardSpeed = 7.0f;
	// 後退速度
	public float backwardSpeed = 2.0f;
	// 旋回速度
	public float rotateSpeed = 2.0f;

    void Reset()
    {
        Head = GetComponentInChildren<OVRCameraRig>().transform.Find("TrackingSpace/CenterEyeAnchor");
    }

    float Scale
    {
        get
        {
            return IsPressTrigger ? DashSpeed : IsPressGrip ? SlowSpeed : 1f;
        }
    }

    bool IsPressTrigger
    {
        get
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
    }
    bool IsPressGrip
    {
        get
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftAlt);
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        // Forward move
        if (Input.GetKey(KeyCode.W))
        {
            var forward = Head.forward;
            forward.y = 0;
            transform.position += forward.normalized * Time.deltaTime * Scale;
        }
        // Back move
        if (Input.GetKey(KeyCode.S))
        {
            var forward = Head.forward;
            forward.y = 0;
            transform.position -= forward.normalized * Time.deltaTime * Scale;
        }
        // Left rotate
        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.Rotate(0, -Angle, 0);
        }
        // Right rotate
        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.Rotate(0, Angle, 0);
        }
        // Up move
        if (Input.GetKeyDown(KeyCode.K))
        {
            transform.position += Vector3.up * Scale;
        }
        // Down move
        if (Input.GetKeyDown(KeyCode.J))
        {
            transform.position -= Vector3.up * Scale;
        }
#else
		// 上下のキー入力で移動する
        float v = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
		var forward = Head.forward;
		forward.y = 0;
		if (v > 0.1) {
	        // Forward move
			transform.position += forward.normalized * Time.deltaTime * forwardSpeed * v;
		} else if (v < -0.1) {
	        // Back move
			transform.position += forward.normalized * Time.deltaTime * backwardSpeed * v;
		}
		// 左右のキー入力でY軸で旋回させる
        // Left rotate
		if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickLeft))
		{
			transform.Rotate(0, -Angle, 0);
		}
        // Right rotate
		if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickRight))
		{
			transform.Rotate(0, Angle, 0);
		}
        // Up move
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            //transform.position += Vector3.up * Scale;
        }
        // Down move
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            //transform.position -= Vector3.up * Scale;
        }
#endif
    }
}
