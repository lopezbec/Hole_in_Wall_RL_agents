using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestingCameras : MonoBehaviour
{
    public RawImage cameraImage;
    private WebCamTexture webcamTexture;

    public RawImage avatarImage;
    public Camera avatarCamera;
    private RenderTexture renderTexture;

    public Canvas canvas;
    public GameObject canvasFollow;
    private float distance = 3f;

    public bool showInXR = true;

    // Start is called before the first frame update
    void Start()
    {
        // Get the default webcam and start playing
        webcamTexture = new WebCamTexture();
        cameraImage.texture = webcamTexture;
        cameraImage.material.mainTexture = webcamTexture;
        webcamTexture.Play();

        //camera for showing the avatars movements
        renderTexture = new RenderTexture(Screen.width, Screen.height, 16);
        avatarCamera.targetTexture = renderTexture;
        avatarImage.texture = renderTexture;

        //moving the images to see the camera images
        if (showInXR) canvas.transform.position = canvasFollow.transform.position + new Vector3(0, 0, distance);
        else canvas.transform.position = new Vector3(14, 3, 3);
    }

    void OnDestroy()
    {
        // Stop the webcam when the object is destroyed
        webcamTexture.Stop();
    }

    private void FixedUpdate()
    {
        if (showInXR)
        {
            Vector3 inFrontOfCamera = canvasFollow.transform.position + canvasFollow.transform.forward * distance;
            //Update the object's position
            canvas.transform.position = inFrontOfCamera;
            //make the object face the same direction as the camera
            canvas.transform.rotation = canvasFollow.transform.rotation;
        }
        //move the camera that is facing the image
        avatarCamera.transform.position = canvasFollow.transform.position + new Vector3(0, 0, 3.05f);
    }
}
