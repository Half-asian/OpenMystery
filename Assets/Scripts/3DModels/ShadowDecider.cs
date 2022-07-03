using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowDecider
{
    public static bool decideShadow(string name, string shader, string modelname)
    {
        if (name.Contains("sky") && !name.Contains("skye"))
            return false;

        if (name.Contains("dome") && modelname != "b_PotionsClassroom_skin")
            return false;

        if (shader == "lightrays_vfx" || shader == "glow_vfx")
            return false;

        return true;
    }
}
