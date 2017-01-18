// Jared White
// September 23, 2016

using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class CameraWatch : MonoBehaviour {

    // Fields
    #region Fields
    public Camera[] viewingCameras;
    public GameObject fpsController;

    private int cameraIndex;
    private int fontSize;
    private string cameraText;
    private string altText;
    private Color color;
    #endregion

    // Use this for initialization
    void Start ()
    {
        // Set the first camera to active, and deactivate all others
        cameraIndex = 0;

	    if (viewingCameras.Length > 0)
        {
            viewingCameras[0].gameObject.SetActive(true);

            for (int i = 1; i < viewingCameras.Length; i++)
            {
                viewingCameras[i].gameObject.SetActive(false);
            }
            
            if (fpsController != null
                && fpsController.GetComponentInChildren<Camera>() != null)
            {
                fpsController.GetComponentInChildren<Camera>().enabled = false;
                fpsController
                    .GetComponent<FirstPersonController>().enabled = false;
            }

            cameraText = viewingCameras[0].name;
            altText = "Use the left and right arrow keys to change "
                + "cameras. Press 'F' to enter first person mode.";
        }
        else
        {
            cameraText = "";
            altText = "";
        }

        // Set the size and color for the GUI
        fontSize = 15;
        color = Color.black;
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Change the active camera based upon user input
        if (viewingCameras.Length > 1)
        {
            if (fpsController != null)
            {
                // Set the FPS controller camera to the opposite active state of
                // what it currently is when the F key is pressed. If FPS is
                // active, viewing cameras cannot be cycled.
                if (Input.GetKeyDown(KeyCode.F))
                {
                    viewingCameras[cameraIndex].gameObject.SetActive(
                        !viewingCameras[cameraIndex].gameObject.activeSelf);

                    if (fpsController.GetComponentInChildren<Camera>().enabled)
                    {
                        fpsController.GetComponentInChildren<Camera>().enabled
                            = false;
                        fpsController
                            .GetComponent<FirstPersonController>().enabled
                            = false;
                        cameraText = viewingCameras[cameraIndex].name;
                        altText = "Use the left and right arrow keys to change "
                            + "cameras. Press 'F' to enter first person mode.";
                    }
                    else
                    {
                        fpsController.GetComponentInChildren<Camera>().enabled
                            = true;
                        fpsController
                            .GetComponent<FirstPersonController>().enabled
                            = true;
                        cameraText = "First Person Mode";
                        altText = "Press 'F' to leave first person mode and "
                            + "return to the viewing cameras.";
                    }
                }

                // If the FPS is active, it must be inactive before cameras
                // can be cycled.
                if (fpsController.GetComponentInChildren<Camera>().enabled)
                {
                    return;
                }
            }

            // Use right arrow key to advance a camera
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                viewingCameras[cameraIndex].gameObject.SetActive(false);
                cameraIndex++;
                if (cameraIndex >= viewingCameras.Length)
                {
                    cameraIndex = 0;
                }

                viewingCameras[cameraIndex].gameObject.SetActive(true);
                cameraText = viewingCameras[cameraIndex].name;
            }
            // Use left arrow key to go back a camera
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                viewingCameras[cameraIndex].gameObject.SetActive(false);
                cameraIndex--;
                if (cameraIndex < 0)
                {
                    cameraIndex = viewingCameras.Length - 1;
                }

                viewingCameras[cameraIndex].gameObject.SetActive(true);
                cameraText = viewingCameras[cameraIndex].name;
            }
        }
	}

    /// <summary>
    /// Draw a GUI to the screen describing what is being viewed.
    /// </summary>
    void OnGUI()
    {
        // Draw a box with the text inside in the upper-left corner
        GUI.skin.box.fontSize = fontSize;
        GUI.skin.box.wordWrap = true;
        GUI.color = color;
        GUI.Box(new Rect(10, 10, 250, 80),
            cameraText + "\n" + altText);
    }
}
