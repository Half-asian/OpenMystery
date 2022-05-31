using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorMovement
{

    public IEnumerator coroutine_walk;
    public Quaternion goal_rotation;

    public string destination_waypoint;
    public Vector3 destination_position;
    public Quaternion destination_rotation;
    public List<string> path;
    const float RUN_SPEED = 1.3f;

    ActorController actor_manager;

    public ActorMovement(ActorController _actor_manager)
    {
        actor_manager = _actor_manager;
    }

    private Transform transform { get { return actor_manager.transform; } }
    private GameObject gameObject { get { return actor_manager.gameObject; } }
    private ActorAnimation actor_animation { get { return actor_manager.actor_animation; } }
    private ActorState actor_state { get { return actor_manager.actor_state; } set { actor_manager.actor_state = value; } }
    private ActorHead actor_head { get { return actor_manager.actor_head; } }
    private ConfigHPActorInfo._HPActorInfo actor_info { get { return actor_manager.actor_info; } }

    public void setDestinationWaypoint(string _waypoint)
    {

        if (!Scene.current.waypoint_dict.ContainsKey(_waypoint))
        {
            Debug.LogError("Could not set character destination waypoint to " + _waypoint + " because it doesn't exist in scene.");
            return;
        }
        destination_waypoint = _waypoint;
        ConfigScene._Scene.WayPoint waypoint = Scene.current.waypoint_dict[destination_waypoint];
        destination_position = new Vector3(waypoint.position[0] * -0.01f, waypoint.position[1] * 0.01f, waypoint.position[2] * 0.01f);
        if (waypoint.rotation != null)
        {
            destination_rotation = Quaternion.Euler(new Vector3(waypoint.rotation[0], waypoint.rotation[1] * -1, waypoint.rotation[2]));
        }
        else
        {
            destination_rotation = Quaternion.Euler(Vector3.zero);
        }
    }

    public void setWaypoint(string _waypoint)
    {
        setDestinationWaypoint(_waypoint);
        transform.position = destination_position;
        transform.rotation = destination_rotation;
    }

    public string getDestinationWaypoint()
    {
        return destination_waypoint;
    }

    private IEnumerator RotateOverTime()
    {
        yield return null;
        while (transform.rotation.eulerAngles != destination_rotation.eulerAngles)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal_rotation, Time.deltaTime * 500);
            yield return null;
        }
        GameStart.current.GetComponent<EventManager>().notifyMoveComplete(gameObject.name);
    }



    private IEnumerator WaitForMove(float speed)
    {
        while (path.Count != 0 || gameObject.transform.position != destination_position)
        {
            if (gameObject.transform.position != destination_position)
            {
                Vector3 targetDirection = transform.position - destination_position;
                transform.rotation = Quaternion.LookRotation((targetDirection).normalized);
                transform.rotation = Quaternion.Euler(new Vector3(0.0f, transform.rotation.eulerAngles.y + 180, 0.0f));
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, destination_position, (0.4f + speed) * Time.deltaTime);
                yield return null;
            }
            else
            {
                destination_waypoint = path[0];
                path.RemoveAt(0);
                ConfigScene._Scene.WayPoint waypoint = Scene.current.waypoint_dict[destination_waypoint];
                destination_position = new Vector3(waypoint.position[0] * -0.01f, waypoint.position[1] * 0.01f, waypoint.position[2] * 0.01f);
                if (waypoint.rotation != null)
                {
                    //destination_rotation = Quaternion.Euler(new Vector3(waypoint.rotation[0], waypoint.rotation[1] * -1, waypoint.rotation[2]));
                    destination_rotation = Quaternion.Euler(0, waypoint.rotation[1] * -1, 0); //Ignore X AND Z
                }
                else
                {
                    destination_rotation = Quaternion.Euler(Vector3.zero);
                }
                Vector3 targetDirection = transform.position - destination_position;

                if (gameObject.transform.position != destination_position) //!(Vector3.Distance(gameObject.transform.position, destination_position) < 0.1f))
                {
                    transform.rotation = Quaternion.LookRotation((targetDirection).normalized);
                    transform.rotation = Quaternion.Euler(new Vector3(0.0f, transform.rotation.eulerAngles.y + 180, 0.0f));
                }
                yield return null;
            }
        }
        if (actor_manager.GetComponent<ActorAnimSequence>() != null)
        {
            if (actor_manager.GetComponent<ActorAnimSequence>().walk == true)
            {
                if (actor_animation.anim_sequence_idle != "")
                {
                    actor_manager.GetComponent<ActorAnimSequence>().initAnimSequence(actor_manager.actor_animation.anim_sequence_idle, false);
                }
                else
                {
                    GameObject.DestroyImmediate(actor_manager.GetComponent<ActorAnimSequence>());
                }
            }
        }


        actor_state = ActorState.Idle;
        goal_rotation = destination_rotation;

        //gameObject.transform.rotation = destination_rotation;
        actor_manager.StartCoroutine(RotateOverTime());

        actor_animation.anim_state = "outro";

        if (actor_manager.GetComponent<ActorAnimSequence>() == null)
        {

            actor_animation.replaceCharacterIdle(actor_info.animId_idle);
            actor_animation.loadAnimationSet();
            actor_animation.updateAnimationState();
        }
    }

    public void moveCharacter(List<string> _path, string animation = "")
    {
        actor_head.clearLookat();
        actor_head.clearTurnHeadAt();

        actor_state = ActorState.Walk;
        if (coroutine_walk != null)
        {
            actor_manager.StopCoroutine(coroutine_walk);
            if (path != null)
            {
                if (path.Count != 0)
                {
                    destination_waypoint = path[0];
                }
            }
        }
        path = _path;
        string path_string = "";
        foreach (string s in path)
        {
            path_string += s + " ";
        }

        setDestinationWaypoint(path[0]);
        path.RemoveAt(0);
        if (animation == actor_info.animId_run || animation == "c_Stu_Jog01")
        {
            coroutine_walk = WaitForMove(RUN_SPEED);
        }
        else
        {
            coroutine_walk = WaitForMove(0.0f);
        }
        actor_animation.setCharacterWalk(animation);
        actor_manager.StartCoroutine(coroutine_walk);
    }

    public void moveCharacterNoAnimation(List<string> _path, float speed)
    {
        actor_head.clearTurnHeadAt();
        actor_head.clearLookat();

        actor_state = ActorState.Walk;
        if (coroutine_walk != null)
        {
            actor_manager.StopCoroutine(coroutine_walk);
            if (path != null)
            {
                if (path.Count != 0)
                {
                    destination_waypoint = path[0];
                }
            }
        }
        path = _path;
        string path_string = "";
        foreach (string s in path)
        {
            path_string += s + " ";
        }

        setDestinationWaypoint(path[0]);
        path.RemoveAt(0);

        coroutine_walk = WaitForMove(speed);
        actor_manager.StartCoroutine(coroutine_walk);
    }

    public void teleportCharacter(Vector3 destination_position, Vector3 destination_rotation)
    {
        if (coroutine_walk != null)
        {
            actor_manager.StopCoroutine(coroutine_walk);
        }
        path = null;
        gameObject.transform.position = destination_position;
        //gameObject.transform.rotation = Quaternion.identity * Quaternion.Euler(destination_rotation);

        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.Rotate(new Vector3(0, 0, -destination_rotation[2]));
        gameObject.transform.Rotate(new Vector3(0, -destination_rotation[1], 0));
        gameObject.transform.Rotate(new Vector3(destination_rotation[0], 0, 0));


        actor_state = ActorState.Idle;
        actor_animation.loadAnimationSet();
        actor_animation.anim_state = "outro";
        actor_animation.updateAnimationState();
    }
}
