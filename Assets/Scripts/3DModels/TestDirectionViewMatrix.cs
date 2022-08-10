using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDirectionViewMatrix : MonoBehaviour
{
    private static float CC_RADIANS_TO_DEGREES(float rads)
    {
        return rads * 57.29577951f;
    }

    private static float CC_DEGREES_TO_RADIANS(float degs)
    {
        return degs * 0.01745329252f;
    }
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < Scene.dirLights.Count; i++) {

            Matrix4x4 matrix = Scene.dirLights[i].preCameraMatrix;


            Matrix4x4 view_matrix = CameraManager.current.main_camera.GetComponent<Camera>().worldToCameraMatrix;

            Vector3 flipz = new Vector3(0, 180, 0);
            Matrix4x4 flipzm = Matrix4x4.Rotate(Quaternion.Euler(flipz));
            view_matrix = view_matrix * flipzm;


            matrix = matrix * view_matrix;

            SkinnedMeshRenderer smr = gameObject.GetComponent<SkinnedMeshRenderer>();

            Vector3 newdirection = new Vector3(matrix[8], -matrix[9], -matrix[10]);
            newdirection.Normalize();

            if (i == 0)
                smr.material.SetVector("u_DirLightSourceDirection1", newdirection);
            if (i == 1)
                smr.material.SetVector("u_DirLightSourceDirection2", newdirection);
            if (i == 2)
                smr.material.SetVector("u_DirLightSourceDirection3", newdirection);
            if (i == 3)
                smr.material.SetVector("u_DirLightSourceDirection4", newdirection);
        }
    }
}
