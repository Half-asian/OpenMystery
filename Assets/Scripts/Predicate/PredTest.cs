using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PredTest : MonoBehaviour
{
    void Start()
    {
        GlobalEngineVariables.assets_folder = "D:\\Hogwarts Mystery\\all assets";

        CocosPU.PUParticleSpawner.spawnParticle("fx_lumos_07_v27.pu", new Vector3(0.01f, 0.01f, 0.01f));
    }

    private void loadAllMaterials()
    {
        var files = Directory.GetFiles(Path.Join(GlobalEngineVariables.assets_folder, "particles"), "*.material");
        foreach (var file in files)
        {
            Debug.Log(file);
            CocosPU.PUMaterial.loadMaterial(file);
        }
    }

    private void loadAllParticles()
    {
        var files = Directory.GetFiles(Path.Join(GlobalEngineVariables.assets_folder, "particles"), "*.pu");
        foreach (var file in files)
        {
            Debug.Log(file);
            CocosPU.PUParticle.loadParticle(file);
        }
    }

    private void spawnAllParticles()
    {
        var files = Directory.GetFiles(Path.Join(GlobalEngineVariables.assets_folder, "particles"), "*.pu");
        foreach (var file in files)
        {
            Debug.Log(file);
            CocosPU.PUParticleSpawner.spawnParticle(file, new Vector3(0.01f, 0.01f, 0.01f));
        }
    }

}
