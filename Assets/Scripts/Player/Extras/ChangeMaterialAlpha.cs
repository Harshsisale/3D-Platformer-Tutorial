using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Changes material Alphas based on distance
public class ChangeMaterialAlpha : MonoBehaviour {


    float minimumDistance = 1.5f;
    float maximumDistance = 2f;

    float minimumAlphaValue = 0.05f;
    float maximumAlphaValue = 0.6f;
    float alphaValue = 0.5f;

	private bool duringInvincible = false;

    void Start()
    {
        if (GameObject.FindObjectOfType<ThirdPersonCamera>())
        {
            cameraTransform = GameObject.FindObjectOfType<ThirdPersonCamera>().transform;
        } else
        {
            cameraTransform = Camera.main.transform;
        }

        if (GameObject.FindObjectOfType<ThirdPersonCharacterController>())
        {
            playerTransform = GameObject.FindObjectOfType<ThirdPersonCharacterController>().transform;
        }
    }

	// Update is called once per frame
	void Update () {
		if (!duringInvincible) {
			UpdateAplhaValueBasedOnDistance();
		}
        ChangeAllChildMaterials();
    }

    Transform cameraTransform;
    Transform playerTransform;
    void UpdateAplhaValueBasedOnDistance()
    {
        if ((cameraTransform!=null) && (playerTransform!=null))
        {
            float distanceCameraAway = (playerTransform.position - cameraTransform.position).magnitude;
            if (distanceCameraAway <= minimumDistance)
            {
                distanceCameraAway = minimumDistance;
            }
            if (distanceCameraAway >= maximumDistance)
            {
                distanceCameraAway = maximumDistance;
            }
            alphaValue = ((distanceCameraAway - minimumDistance) / (maximumDistance - minimumDistance)) * (maximumAlphaValue + minimumAlphaValue);
            if (alphaValue < minimumAlphaValue)
            {
                alphaValue = minimumAlphaValue;
            }
        }
        
    }

    void ChangeAllChildMaterials()
    {
        List<MaterialSwap> materialSwappers = new List<MaterialSwap>(gameObject.GetComponentsInChildren<MaterialSwap>());

        if (alphaValue > maximumAlphaValue)
        {
            foreach (MaterialSwap materialSwap in materialSwappers)
            {
                materialSwap.GoToOpaque();
                Renderer renderer = materialSwap.GetComponent<Renderer>();
                foreach(Material material in renderer.materials)
                {
                    Color materialColor = material.color;
                    material.color = new Color(materialColor.r, materialColor.g, materialColor.b, 1);
                }

            }
        }
        else
        {
            foreach (MaterialSwap materialSwap in materialSwappers)
            {
                materialSwap.GoToTransparent();
                Renderer renderer = materialSwap.GetComponent<Renderer>();
                foreach (Material material in renderer.materials)
                {
                    Color materialColor = material.color;
                    material.color = new Color(materialColor.r, materialColor.g, materialColor.b, alphaValue);
                }
            }
        }      
    }

	public void setDuringInvincible(bool isInvincible){
		duringInvincible = isInvincible;
	}
}
