using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneTransform
{
    public string boneName;
    public Vector3 translation;
    public Quaternion rotation;
    public Vector3 scale;

    public BoneTransform(Vector3 _t, Quaternion _r, Vector3 _s)
    {
        translation = _t;
        rotation = _r;
        scale = _s;
    }

    public void apply(ref Transform transform)
    {
        transform.position += translation;
        transform.rotation *= rotation;
        transform.localScale.Scale(scale);
    }

    public void apply(ref Matrix4x4 matrix)
    {
        

        Vector3 og_pos = ModelManager.ExtractTranslationFromMatrix(ref matrix);
        Debug.Log("OG POS: " + og_pos);
        Quaternion og_rot = ModelManager.ExtractRotationFromMatrix(ref matrix);
        Debug.Log("OG_ROT: " + og_rot);
        Vector3 og_scale = ModelManager.ExtractScaleFromMatrix(ref matrix);
        Debug.Log("OG_SCALE: " + og_scale);

        //Vector3 new_pos = new Vector3(og_pos.x + translation.x, og_pos.y + translation.y, og_pos.z + translation.z);
        //Quaternion new_rot = og_rot * rotation;
        //Vector3 new_scale = new Vector3(og_scale.x * scale.x, og_scale.y * scale.y, og_scale.z * scale.z);

        //Matrix4x4 mtranslation = Matrix4x4.Translate(translation);
        //Matrix4x4 mrotation = Matrix4x4.Rotate(rotation);

        Vector3 xx = new Vector3(og_pos.x * 5, og_pos.y * 5, og_pos.z * 5);

        matrix = Matrix4x4.TRS(xx, og_rot, new Vector3(5, 5, 5));

        Vector3 new_pos = ModelManager.ExtractTranslationFromMatrix(ref matrix);
        Debug.Log("NEW POS: " + new_pos);
        Quaternion new_rot = ModelManager.ExtractRotationFromMatrix(ref matrix);
        Debug.Log("NEW_ROT: " + new_rot);
        Vector3 new_scale = ModelManager.ExtractScaleFromMatrix(ref matrix);
        Debug.Log("NEW_SCALE: " + new_scale);

        //matrix = matrix.;

        //Matrix4x4 mscale = Matrix4x4.Scale(scale);

        //matrix *= mtranslation;
        //matrix *= mrotation;
        //matrix *= mscale;
        //matrix = matrix.inverse;

    }
}

