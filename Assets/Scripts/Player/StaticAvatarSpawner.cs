using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModelLoading;

class StaticAvatarSpawner
{
    public static ActorController spawnStaticAvatar(ConfigHPActorInfo._HPActorInfo actor = null)
    {
        //Get the Avatar components
        AvatarComponents avatar_components = new AvatarComponents(Path.Combine(GlobalEngineVariables.player_folder, "Avatar.json"));
        ConfigHPActorInfo._HPActorInfo character;
        if (actor == null)
        {
            //Get the character. Spawn the animator.
            character = Configs.config_hp_actor_info.HPActorInfo[PlayerManager.current.character_id];

        }
        else
        {
            character = actor;
        }

        Debug.Log(character.modelId);
        Model parent_model;

        if (PlayerManager.current.gender == "male")
        {
            parent_model = ModelManager.loadModel("c_Avatar_Male_headNoseless_skin");
            avatar_components.gender = "male";
        }
        else
        {
            parent_model = ModelManager.loadModel("c_Avatar_Female_headNoseless_skin");
            avatar_components.gender = "female";

        }


        parent_model.game_object.name = "Avatar";


        ActorController c = parent_model.game_object.AddComponent<ActorController>();
        c.setup(parent_model);
        c.actor_info = character;
        c.actor_animation.animId_idle = character.animId_idle;
        c.patches = new List<Model>();
        c.avatar_components = avatar_components;

        avatar_components.setCharacterManager(c);
        avatar_components.base_model = parent_model;
        avatar_components.spawnComponents();

        avatar_components.base_model.game_object.transform.parent = parent_model.game_object.transform;

        if (character.modelPatches != null)
        {
            foreach (string patch in character.modelPatches)
            {
                Model patch_model = ModelManager.loadModel(patch, avatar_components.base_model.pose_bones);

                if (patch_model.game_object != null)
                {
                    patch_model.game_object.transform.parent = parent_model.game_object.transform;
                    c.patches.Add(patch_model);

                }
            }
        }



        c.actor_animation.bone_mods = avatar_components.bonemods;
        c.actor_animation.replaceCharacterIdle(c.actor_info.animId_idle);
        c.actor_animation.setCharacterIdle();





        return c;
    }
}

