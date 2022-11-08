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

        ActorController actor_controller = character_model.game_object.AddComponent<ActorController>();
        actor_controller.setup(character_model);
        actor_controller.config_hpactor = character;
        actor_controller.replaceCharacterIdle(character.animId_idle);
        actor_controller.patches = new List<Model>();
        actor_controller.bone_mods = new Dictionary<string, AnimationManager.BoneMod>();

        if (character_model.game_object != null)
        {
            character_model.game_object.name = character_name;
        }

        actor_controller.replaceCharacterIdle(actor_controller.config_hpactor.animId_idle);
        return actor_controller;
    }
}
