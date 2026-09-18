using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Slider horizontalMouseSensitivitySlider;
    public Slider verticalMouseSensitivitySlider;

    public Toggle invertX;
    public Toggle invertY;

    public ThirdPersonCamera playerCamera;

    private void OnEnable()
    {
        invertX.SetIsOnWithoutNotify(PlayerPrefs.GetInt("InvertX", 0) != 0);
        invertY.SetIsOnWithoutNotify(PlayerPrefs.GetInt("InvertY", 0) != 0);

        horizontalMouseSensitivitySlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat("HorizontalMouseSensitivity", horizontalMouseSensitivitySlider.value));
        verticalMouseSensitivitySlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat("VerticalMouseSensitivity", verticalMouseSensitivitySlider.value));

        PlayerPrefs.SetFloat("HorizontalMouseSensitivity", horizontalMouseSensitivitySlider.value);
        PlayerPrefs.SetFloat("VerticalMouseSensitivity", verticalMouseSensitivitySlider.value);
        ApplyCameraSettings();
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }

    public void ChangeHorizontalMouseSensitivity()
    {
        PlayerPrefs.SetFloat("HorizontalMouseSensitivity", horizontalMouseSensitivitySlider.value);
        ApplyCameraSettings();
    }

    public void ChangeVerticalMouseSensitivity()
    {
        PlayerPrefs.SetFloat("VerticalMouseSensitivity", verticalMouseSensitivitySlider.value);
        ApplyCameraSettings();
    }

    public void ChangeInvertX()
    {
        PlayerPrefs.SetInt("InvertX", invertX.isOn ? 1 : 0);
        ApplyCameraSettings();
    }

    public void ChangeInvertY()
    {
        PlayerPrefs.SetInt("InvertY", invertY.isOn ? 1 : 0);
        ApplyCameraSettings();
    }

    private void ApplyCameraSettings()
    {
        foreach (CinemachineCameraSettings cameraSettings in FindObjectsOfType<CinemachineCameraSettings>(true))
        {
            cameraSettings.ApplySettings();
        }

        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<ThirdPersonCamera>();
        }

        if (playerCamera != null)
        {
            playerCamera.InitialSetup();
        }
    }
}
