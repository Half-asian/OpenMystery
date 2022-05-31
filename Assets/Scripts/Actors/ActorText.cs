using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorText : MonoBehaviour
{
    TextMesh tm;
    Transform ct;
    void Start()
    {
        tm = GetComponent<TextMesh>();
        ct = CameraManager.current.main_camera.GetComponent<Transform>();
    }

    private void Update()
    {
        FaceTextMeshToCamera(tm, ct);
    }

    void FaceTextMeshToCamera(TextMesh textMeshObject, Transform textLookTargetTransform)
    {
        Vector3 origRot = textMeshObject.transform.eulerAngles;
        textMeshObject.transform.LookAt(textLookTargetTransform);
        Vector3 desiredRot = textMeshObject.transform.eulerAngles + new Vector3(0, 180, 0);
        origRot.y = desiredRot.y;
        textMeshObject.transform.eulerAngles = origRot;
    }
}
