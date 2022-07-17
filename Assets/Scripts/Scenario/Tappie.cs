using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tappie
{
    private static List<GameObject> tappies = new List<GameObject>();

    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
        Scenario.onScenarioLoaded += cleanup;
    }

    private static void cleanup()
    {
        foreach (GameObject tappie in tappies)
            GameObject.Destroy(tappie);
        tappies.Clear();
    }

    public static void spawnTappies()
    {
        if (Scenario.current.scenario_config.tappies == null)
            return;
        foreach(string tappie_id in Scenario.current.scenario_config.tappies)
        {
            if (!Configs.config_tappie.Tappie.ContainsKey(tappie_id))
                continue;
            ConfigTappie._Tappie tappie = Configs.config_tappie.Tappie[tappie_id];
            if (tappie.showPredicate != null)
                if (!Predicate.parsePredicate(tappie.showPredicate))
                    continue;

            if (!Scene.current.waypoint_dict.ContainsKey(tappie.activeWaypoint))
                continue;

            GameObject tappiego = new GameObject(tappie_id);
            Model m = ModelManager.loadModel(tappie.activeModel);
            if (tappie.activeAnimation != null)
            {
                AnimationClip anim = AnimationManager.loadAnimationClip(tappie.activeAnimation, m, null);
                Animation animation_component = m.game_object.AddComponent<Animation>();
                animation_component.AddClip(anim, "default");
                animation_component.Play("default");
            }
            if (tappie.activeSequence != null)
            {
                ActorController actor_controller = m.game_object.AddComponent<ActorController>();
                actor_controller.setup(m);
                ActorAnimSequence animseq_component = m.game_object.AddComponent<ActorAnimSequence>();
                animseq_component.initAnimSequence(tappie.activeSequence, false);
            }

            m.game_object.transform.parent = tappiego.transform;

            Common.setWaypointTransform(ref tappiego, Scene.current.waypoint_dict[tappie.activeWaypoint]);
            tappies.Add(tappiego);
        }
    }


}
