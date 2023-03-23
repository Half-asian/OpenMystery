using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariantManager
{



    private static string variant = null;

    public static void setVariant(string _variant)
    {
        variant = _variant;
    }

    public static void removeVariant()
    {
        variant = null;
    }

    public static string getVariantForId(string id) {
        if (variant != null)
            return id + "+" + variant;
        return id;
    }

}
