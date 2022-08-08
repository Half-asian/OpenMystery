using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelLoading;

public class CustomizableAvatarSpawner : MonoBehaviour
{
    public static ActorController spawnCustomizableAvatar(string character_id, string character_name)
    {
        ConfigHPActorInfo._HPActorInfo character = null;

        character = Configs.config_hp_actor_info.HPActorInfo[character_id];

        Model character_model = null;

        if (PlayerManager.current.gender == "male")
        {
            character_model = ModelManager.loadModel("c_Avatar_Male_headNoseless_skin");
        }
        else
        {
            character_model = ModelManager.loadModel("c_Avatar_Female_headNoseless_skin");
        }


        Debug.Log("spawning avatar");
        character_model.game_object.name = character_name;

        ActorController c = character_model.game_object.AddComponent<ActorController>();
        c.setup(character_model);
        c.actor_info = character;
        c.actor_animation.animId_idle = character.animId_idle;
        c.patches = new List<Model>();
        c.actor_animation.bone_mods = new Dictionary<string, AnimationManager.BoneMod>();

        if (character_model.game_object != null)
        {
            character_model.game_object.name = character_name;
        }

        c.actor_animation.replaceCharacterIdle(c.actor_info.animId_idle);
        c.actor_animation.setCharacterIdle();
        return c;
    }
}
