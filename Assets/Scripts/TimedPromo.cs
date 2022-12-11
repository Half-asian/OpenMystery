using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedPromo : MonoBehaviour
{
    public static bool isTimedPromoActive(string timed_promo_id)
    {
        if (timed_promo_id == "christmas2022_theming" || timed_promo_id == "winter2022_theming")
        {
            return true;
        }
        return false;
    }
}
