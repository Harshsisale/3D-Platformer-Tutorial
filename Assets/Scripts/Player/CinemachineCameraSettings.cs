using Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CinemachineFreeLook))]
public class CinemachineCameraSettings : MonoBehaviour
{
    private CinemachineFreeLook freeLook;
    private float defaultHorizontalSpeed;
    private float defaultVerticalSpeed;
    private bool defaultInvertX;
    private bool defaultInvertY;

    private void OnEnable()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        if (freeLook == null)
        {
            freeLook = GetComponent<CinemachineFreeLook>();
            defaultHorizontalSpeed = freeLook.m_XAxis.m_MaxSpeed;
            defaultVerticalSpeed = freeLook.m_YAxis.m_MaxSpeed;
            defaultInvertX = freeLook.m_XAxis.m_InvertInput;
            defaultInvertY = freeLook.m_YAxis.m_InvertInput;
        }

        freeLook.m_XAxis.m_InvertInput = defaultInvertX ^ (PlayerPrefs.GetInt("InvertX", 0) != 0);
        freeLook.m_YAxis.m_InvertInput = defaultInvertY ^ (PlayerPrefs.GetInt("InvertY", 0) != 0);
        freeLook.m_XAxis.m_MaxSpeed = defaultHorizontalSpeed * PlayerPrefs.GetFloat("HorizontalMouseSensitivity", 1f);
        freeLook.m_YAxis.m_MaxSpeed = defaultVerticalSpeed * PlayerPrefs.GetFloat("VerticalMouseSensitivity", 1f);
    }
}
