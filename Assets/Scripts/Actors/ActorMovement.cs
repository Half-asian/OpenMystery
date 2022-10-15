using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class ActorController : Node
{

    private IEnumerator coroutine_move;
    private ConfigScene._Scene.WayPoint destination_waypoint;
    private bool is_moving = false;

    public string getDestinationWaypoint()
    {
        if (destination_waypoint == null)
        {
            Debug.LogError("DESTINATION WAYPOINT WAS NULL");
            return null;

        }
        return destination_waypoint.name;
    }

    public void finishMovement()
    {
        if (is_moving == false)
            return;
        is_moving = false;

        if (coroutine_move != null) //Early finish
        {
            StopCoroutine(coroutine_move);
            applyWaypoint();
            if (GameStart.current != null)
                GameStart.current.GetComponent<EventManager>().notifyMoveComplete(gameObject.name);
            coroutine_move = null;
        }
    }


    public void moveCharacter(List<string> path, float speed)
    {
        finishMovement();

        destination_waypoint = Scene.current.waypoint_dict[path.Last()];

        actor_head.clearLookat();
        actor_head.clearTurnHeadAt();

        coroutine_move = MoveCoroutine(path, speed);
        StartCoroutine(coroutine_move);
        setCharacterWalk();
    }

    public void teleportCharacter(string waypoint_id)
    {
        if (waypoint_id == null || !Scene.current.waypoint_dict.ContainsKey(waypoint_id))
            return;

        finishMovement();

        destination_waypoint = Scene.current.waypoint_dict[waypoint_id];

        applyWaypoint();

        setCharacterIdle();
    }

    private void applyWaypoint()
    {
        if (destination_waypoint == null)
            return;
        Vector3 position = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        if (destination_waypoint.position != null)
            position = destination_waypoint.getWorldPosition();
        gameObject.transform.position = position;

        if (destination_waypoint.rotation != null)
            rotation = new Vector3(destination_waypoint.rotation[0], destination_waypoint.rotation[1], destination_waypoint.rotation[2]);

        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.Rotate(new Vector3(0, 0, -rotation[2]));
        gameObject.transform.Rotate(new Vector3(0, -rotation[1], 0));
        gameObject.transform.Rotate(new Vector3(rotation[0], 0, 0));

        if (destination_waypoint.scale != null)
            gameObject.transform.localScale = new Vector3(destination_waypoint.scale[0] * 0.01f, destination_waypoint.scale[1] * 0.01f, destination_waypoint.scale[2] * 0.01f);
    }


    /*----------        COROUTINES      ----------*/

    private IEnumerator MoveCoroutine(List<string> path, float speed)
    {
        is_moving = true;

        ConfigScene._Scene.WayPoint next_waypoint = Scene.current.waypoint_dict[path[0]];
            
        while (path.Count != 0 || gameObject.transform.position != destination_waypoint.getWorldPosition())
        {
            if (gameObject.transform.position != next_waypoint.getWorldPosition())
            {
                Vector3 targetDirection = transform.position - next_waypoint.getWorldPosition();
                transform.rotation = Quaternion.LookRotation((targetDirection).normalized);
                transform.rotation = Quaternion.Euler(new Vector3(0.0f, transform.rotation.eulerAngles.y + 180, 0.0f));
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, next_waypoint.getWorldPosition(), (0.4f + speed) * Time.deltaTime);
                yield return null;
            }
            else
            {
                next_waypoint = Scene.current.waypoint_dict[path[0]];
                path.RemoveAt(0);
            }
        }

        yield return null;

        //Rotate towards the final facing
        Quaternion rotation = Quaternion.identity;
        if (destination_waypoint.rotation != null)
            rotation = Quaternion.Euler(destination_waypoint.getRotation());

        while (transform.rotation != rotation) //Quaternions can have multiple variations for one facing
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * 500);
            yield return null;
        }
        setCharacterIdle();
    }

}
