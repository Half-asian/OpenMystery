using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
public class ModelViewerScreenShot : MonoBehaviour
{
    private RawImage _image;

    int screenshot_num;
    public Camera MyCamera;

    int _resWidth = 1000;
    int _resHeight = 1000;

    void takeScreenshot() {
        int resWidthN = _resWidth;
        int resHeightN = _resHeight;
        RenderTexture rt = new RenderTexture(resWidthN, resHeightN, 24);
        if (MyCamera != null)
        {

            MyCamera.targetTexture = rt;
            TextureFormat tFormat = TextureFormat.ARGB32;
            Texture2D screenShot = new Texture2D(resWidthN, resHeightN, tFormat, false);
            MyCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
            MyCamera.targetTexture = null;
            RenderTexture.active = null;
            byte[] bytes = screenShot.EncodeToPNG();
            string[] filename_pieces = {GlobalEngineVariables.player_folder, "..", "..", "screenshots", screenshot_num.ToString(), ".png"};
            string filename = Path.Combine(filename_pieces);

            File.WriteAllBytes(filename, bytes);
            Debug.Log($"Took screenshot to: {filename}");
            Application.OpenURL(filename);

        }

        //camera.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("f12"))
        {
            for (int i = 1; i <= 9999; i++)
            {
                string[] filename_pieces = {GlobalEngineVariables.player_folder, "..", "..", "screenshots", i.ToString(), ".png"};
                if (!System.IO.File.Exists(Path.Combine(filename_pieces)))
                {
                    //camera.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    screenshot_num = i;
                    takeScreenshot();
                    break;
                }
            }
        }
    }
}
