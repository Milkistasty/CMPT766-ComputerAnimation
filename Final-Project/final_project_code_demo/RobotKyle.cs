// Student ID: 301586596
// Student Name: Wenhe Wang
// Student Email: wwa118@sfu.ca

using UnityEngine;

public class RobotKyle : MonoBehaviour
{
    private float m_RotateAngle = 0.1f;

    private void Start()
    {
        var glitchControl = transform.Find("Robot2").gameObject.AddComponent<GlitchControl>();
    }

    private Vector3 m_Angle;
    private void Update()
    {
        transform.Rotate(Vector3.up, m_RotateAngle, Space.Self);
    }
}
