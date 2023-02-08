using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static ConfigGoal;

namespace UI
{
    class GoalPopup : MonoBehaviour
    {
        [SerializeField]
        GameObject _popup;
        [SerializeField]
        Text _title;
        [SerializeField]
        Text _description;
        [SerializeField]
        Vector3 character_spawn_pos;
        ActorController character = null;
        public ConfigGoal.Goal latest_goal;

        public void setPopup(ConfigGoal.Goal goal)
        {
            if (goal == null)
                throw new Exception("setPopup goal was null");
            latest_goal = goal;

            if (character != null)
            {
                Actor.destroyCharacter(character.name);
                character = null;
            }
            _popup.SetActive(true);

            if (goal.ready_text != null) _description.text = LocalData.getLine(goal.ready_text); else _description.text = "";
            if (goal.goal_name != null) _title.text = LocalData.getLine(goal.goal_name); else _title.text = "";


            StartCoroutine(spawnCharacter(goal));
        }

        private IEnumerator spawnCharacter(ConfigGoal.Goal goal)
        {
            yield return null;
            if (goal.characterId != null)
            {
                Debug.Log("Spawning character id " + goal.characterId);
                character = Actor.spawnActor(goal.characterId, null, goal.characterId);
            }
            if (character == null)
                throw new System.Exception("character was null");
            if (character.gameObject == null)
                throw new System.Exception("character gameobject was null");
            character.gameObject.transform.position = new Vector3(0, -200, 0);
            float new_height = -100f;
            if (character != null && character.model.pose_bones.ContainsKey("jt_head_bind"))
            {
                Vector3 head_position = character.model.pose_bones["jt_head_bind"].position;

                character.gameObject.transform.position = head_position;
                new_height -= head_position.y + 200.0f;
            }
            character.gameObject.transform.position = new Vector3(0, new_height, -0.5f - character.gameObject.transform.position.z);



            foreach (SkinnedMeshRenderer smr in character.gameObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                smr.renderingLayerMask = 2;
            }
        }

        public void closePopup()
        {
            _popup.SetActive(false);
            if (character != null)
            {
                Actor.destroyCharacter(character.name);
                character = null;
            }
        }
    }
}
