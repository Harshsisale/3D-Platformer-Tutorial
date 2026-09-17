using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Slider horizontalMouseSensitivitySlider;
    public Slider verticalMouseSensitivitySlider;

    public Toggle invertX;
    public Toggle invertY;

    public ThirdPersonCamera playerCamera;

    // Start is called before the first frame update
    void Start()
    {
        if (playerCamera==null)
        {
            playerCamera = FindObjectOfType<ThirdPersonCamera>();
        }

        if (PlayerPrefs.HasKey("InvertX"))
        {
            if (PlayerPrefs.GetInt("InvertX") != 0)
            {
                invertX.isOn = true;
            } else
            {
                invertX.isOn = false;
            }
        }
        else
        {
            invertX.isOn = false;
        }

        if (PlayerPrefs.HasKey("InvertY"))
        {
            if (PlayerPrefs.GetInt("InvertY") != 0)
            {
                invertY.isOn = true;
            }
            else
            {
                invertY.isOn = false;
            }
        }
        else
        {
            invertY.isOn = false;
        }

        if (PlayerPrefs.HasKey("HorizontalMouseSensitivity"))
        {
            horizontalMouseSensitivitySlider.value = PlayerPrefs.GetFloat("HorizontalMouseSensitivity");
        }
        else
        {
            PlayerPrefs.SetFloat("HorizontalMouseSensitivity", horizontalMouseSensitivitySlider.value);
        }

        if (PlayerPrefs.HasKey("VerticalMouseSensitivity"))
        {
            verticalMouseSensitivitySlider.value = PlayerPrefs.GetFloat("VerticalMouseSensitivity");
        }
        else
        {
            PlayerPrefs.SetFloat("VerticalMouseSensitivity", verticalMouseSensitivitySlider.value);
        }

        if (PlayerPrefs.HasKey("HorizontalMouseSensitivity"))
        {
            horizontalMouseSensitivitySlider.value = PlayerPrefs.GetFloat("HorizontalMouseSensitivity");
        }
        else
        {
            PlayerPrefs.SetFloat("HorizontalMouseSensitivity", horizontalMouseSensitivitySlider.value);
        }
    }

    public void ChangeHorizontalMouseSensitivity()
    {
        PlayerPrefs.SetFloat("HorizontalMouseSensitivity", horizontalMouseSensitivitySlider.value);
        playerCamera.InitialSetup();
    }

    public void ChangeVerticalMouseSensitivity()
    {
        PlayerPrefs.SetFloat("VerticalMouseSensitivity", verticalMouseSensitivitySlider.value);
        playerCamera.InitialSetup();
    }

    public void ChangeInvertX()
    {
        PlayerPrefs.SetInt("InvertX", (invertX.isOn ? 1 : 0));
        if (playerCamera != null)
        {
            playerCamera.InitialSetup();
        }
    }

    public void ChangeInvertY()
    {
        PlayerPrefs.SetInt("InvertY", (invertY.isOn ? 1 : 0));
        if (playerCamera != null)
        {
            playerCamera.InitialSetup();
        }
    }
}
