using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
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

        Vector3 normal_center = new Vector3(0, -100.4f, -0.3f);
        Vector3 normal_extents = new Vector3(0.4f, 0.48f, 0.2f);
        public void setPopup(ConfigGoal.Goal goal)
        {
            if (goal == null)
                throw new Exception("setPopup goal was null");
            latest_goal = goal;

            if (character != null)
                Destroy(character.gameObject);
            _popup.SetActive(true);

            if (goal.ready_text != null) _description.text = LocalData.getLine(goal.ready_text); else _description.text = "";
            if (goal.goal_name != null) _title.text = LocalData.getLine(goal.goal_name); else _title.text = "";
            if (goal.characterId != null) character = Actor.spawnActor(goal.characterId, null, goal.characterId);

            StartCoroutine(setPos());
        }

        private IEnumerator setPos()
        {
            yield return null;
            character.gameObject.transform.position = new Vector3(0, -200, 0);
            float new_height = -100f;
            if (character != null && character.model.pose_bones.ContainsKey("jt_head_bind"))
            {
                Vector3 head_position = character.model.pose_bones["jt_head_bind"].position;

                character.gameObject.transform.position = head_position;
                new_height -= head_position.y + 200.0f;
                /*Bounds b = Common.getBounds(character.gameObject);
                Debug.Log(b);
                //character.gameObject.transform.Translate(b.center - normal_center);
                character.gameObject.transform.Translate(new Vector3(0, -(b.extents.y - normal_extents.y) * 2, -0.1f));*/


                //character_gameobject.layer = 6; //3d Menu Layer

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
                Destroy(character.gameObject);

        }
    }
}
