using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class ActorController : Node
{

    private IEnumerator coroutine_move;
    private IEnumerator coroutine_rotate;

    private ConfigScene._Scene.WayPoint destination_waypoint;
    private bool is_moving = false;
    List<string> movement_path = new List<string>();
    public string destination_waypoint_name;

    public List<string> carvePath(List<string> visited, string destination)
    {
        List<string> final_result = new List<string>();

        if (Scene.current.waypointconnections == null)
        {
            return final_result;
        }

        foreach (ConfigScene._Scene.WayPointConnection connection in Scene.current.waypointconnections)
        {
            //Check if the waypoint connection defines invalid waypoints
            if (!Scene.isValidWayPoint(connection.connection[0]) || !Scene.isValidWayPoint(connection.connection[1]))
                continue;

            if (connection.connection[0] == visited[visited.Count - 1])
            {
                if (connection.connection[1] == destination)
                {
                    visited.Add(destination);
                    return visited;
                }
                else if (!visited.Contains(connection.connection[1]))
                {
                    List<string> temp = new List<string>(visited);
                    temp.Add(connection.connection[1]);
                    List<string> result = carvePath(temp, destination);

                    if (result[result.Count - 1] == destination) //we found a path
                    {
                        string s_path = "";
                        foreach (string s in result)
                        {
                            s_path += s + " ";
                        }
                        if (final_result.Count != 0) //we already found a path
                        {
                            if (result.Count < final_result.Count)
                            {
                                final_result = result; //The new path is shorter
                            }
                        }
                        else
                        {

                            final_result = result; //we hadn't found a path, now we have
                        }
                    }
                }
            }
            else if (connection.connection[1] == visited[visited.Count - 1])
            {
                if (connection.connection[0] == destination)
                {
                    visited.Add(destination);
                    return visited;
                }
                else if (!visited.Contains(connection.connection[0]))
                {
                    List<string> temp = new List<string>(visited);
                    temp.Add(connection.connection[0]);
                    List<string> result = carvePath(temp, destination);

                    if (result[result.Count - 1] == destination) //we found a path
                    {
                        string s_path = "";
                        foreach (string s in result)
                        {
                            s_path += s + " ";
                        }
                        if (final_result.Count != 0) //we already found a path
                        {
                            if (result.Count < final_result.Count)
                            {
                                final_result = result; //The new path is shorter
                            }
                        }
                        else
                        {
                            final_result = result; //we hadn't found a path, now we have
                        }
                    }
                }
            }
        }
        if (final_result.Count == 0)
        {
            return visited;
        }
        return final_result;
    }

    public void walkInCharacter(string destination_waypoint_id)
    {
        if (!Scene.isValidWayPoint(destination_waypoint_id))
        {
            Debug.LogError("COULDN'T FIND WAYPOINT " + destination_waypoint_id + " IN CURRENT SCENE!");
            return;
        }
        string waypoint_connector = null;
        if (Scene.current.waypointconnections != null)
        {
            foreach (var w in Scene.current.waypointconnections)
            {
                if (w.connection[1] == destination_waypoint_id)
                    waypoint_connector = w.connection[0];
                else if (w.connection[0] == destination_waypoint_id)
                    waypoint_connector = w.connection[1];
            }
        }
        if (waypoint_connector == null)
        {
            Debug.LogError("Couldn't find a connector to waypoint " + destination_waypoint_id);
        }
        else
        {
            teleportCharacter(waypoint_connector);
        }
        moveCharacter(
            destination_waypoint_id,
            null, 
            ActorAnim.AnimType.Regular);
    }

    public void moveCharacter(string destination_waypoint_id, string actor_anim_id, ActorAnim.AnimType anim_type)
    {
        if (destination_waypoint_id == null)
            return;
        if (!Scene.isValidWayPoint(destination_waypoint_id))
        {
            Debug.LogWarning("COULDN'T FIND WAYPOINT " + destination_waypoint_id + " IN CURRENT SCENE! for character move");
            return;
        }

        List<string> path;
        List<string> visited = new List<string>();
        string current_waypoint = destination_waypoint_name;
        if (current_waypoint != null)
        {
            visited.Add(current_waypoint);
            if (current_waypoint == destination_waypoint_id) //We are already at the destination
                return;

            path = carvePath(visited, destination_waypoint_id);

            string s_path = "";
            foreach (string s in path)
            {
                s_path += s + " ";
            }

            Debug.Log(name + " Walking: " + s_path + ". Starting at " + destination_waypoint_name + " going to " + destination_waypoint_id);

            if (path.Count != 0)
            {
                if (path[path.Count - 1] != destination_waypoint_id)//Did not find a path
                {
                    path.Clear();
                    path.Add(destination_waypoint_id); //Change to a direct route
                }
            }
            else
            {
                path.Clear();
                path.Add(destination_waypoint_id); //Change to a direct route
            }
        }
        else
        {
            path = new List<string>() { destination_waypoint_id }; //If actor has no defined waypoint, we move directly to new waypoint. Tested in retail.
        }

        Debug.Log("moveCharacter: " + name);
        if (Scene.getWayPointData(path.Last(), out destination_waypoint))
            destination_waypoint_name = destination_waypoint.name;
        else
            destination_waypoint_name = "";

        movement_path.AddRange(path);

        if (is_moving == false)
        {
            clearAllLooking();
            coroutine_move = MoveCoroutine();
            StartCoroutine(coroutine_move);
            setCharacterWalk();
        }

        if (!string.IsNullOrEmpty(actor_anim_id)) //There is an animation included
        {
            if (anim_type == ActorAnim.AnimType.Sequence)
                replaceCharacterWalkSequence(actor_anim_id);
            else
                replaceCharacterWalk(actor_anim_id);
        }
        else if (config_hpactor.animId_walk != null)
        {
            replaceCharacterWalk(config_hpactor.animId_walk);
        }
    }

    public void finishMovement()
    {
        if (is_moving == false)
            return;
        is_moving = false;
        movement_path.Clear();

        if (coroutine_move != null) //Early finish
        {
            StopCoroutine(coroutine_move);
            Scene.setGameObjectToWaypoint(gameObject, destination_waypoint_name);
            coroutine_move = null;
            if (GameStart.current != null)
                GameStart.current.GetComponent<EventManager>().notifyMoveComplete(gameObject.name);
        }

    }

    public void teleportCharacter(string waypoint_id)
    {


        if (waypoint_id == null || !Scene.isValidWayPoint(waypoint_id))
            return;

        finishMovement();
        if (coroutine_rotate != null)
        {
            StopCoroutine(coroutine_rotate);
            coroutine_rotate = null;
        }

        if (Scene.getWayPointData(waypoint_id, out destination_waypoint))
            destination_waypoint_name = destination_waypoint.name;
        else
            destination_waypoint_name = "";

        Scene.setGameObjectToWaypoint(gameObject, waypoint_id);

        cancel_crossfade = true;

        setCharacterIdle();
        clearAllLooking();

    }

    /*----------        COROUTINES      ----------*/

    private IEnumerator MoveCoroutine()
    {
        if (coroutine_rotate != null)
            StopCoroutine(coroutine_rotate);

        is_moving = true;

        Scene.getWayPointData(movement_path[0], out ConfigScene._Scene.WayPoint next_waypoint);

        Vector3 target_direction = transform.position - next_waypoint.getWorldPosition();
        if (target_direction != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation((target_direction).normalized);
            rotation = Quaternion.Euler(new Vector3(0.0f, rotation.eulerAngles.y + 180, 0.0f));
            gameObject.transform.rotation = rotation;
            startRotateCoroutine(rotation);
        }

        while (gameObject.transform.position != destination_waypoint.getWorldPosition())
        {
            float speed = 0.0f;
            if (animation_current_loop != null)
                speed = animation_current_loop.speed * 0.01f;
            else if (GetComponent<ActorAnimSequence>() != null && GetComponent<ActorAnimSequence>().current_animation != null)
                speed = GetComponent<ActorAnimSequence>().current_animation.speed * 0.01f;
            if (speed <= 0.001f)
                speed = 0.4479245f;
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, next_waypoint.getWorldPosition(), speed * Time.deltaTime);

            if (Vector3.Distance(gameObject.transform.position, next_waypoint.getWorldPosition()) < 0.2f)
            {
                refreshLookAts(next_waypoint.getRotation());
            }

            if (gameObject.transform.position == next_waypoint.getWorldPosition())
            {
                if (movement_path.Count != 0)
                {

                    Scene.getWayPointData(movement_path[0], out next_waypoint);
                    movement_path.RemoveAt(0);
                    target_direction = transform.position - next_waypoint.getWorldPosition();
                    if (target_direction != Vector3.zero)
                    {
                        Quaternion rotation = Quaternion.LookRotation((target_direction).normalized);
                        rotation = Quaternion.Euler(new Vector3(0.0f, rotation.eulerAngles.y + 180, 0.0f));
                        startRotateCoroutine(rotation);
                    }
                }
            }

            yield return null;
        }

        yield return null;

        //Rotate towards the final facing
        Quaternion final_rotation = Quaternion.identity;
        if (destination_waypoint.rotation != null)
        {
            final_rotation = Quaternion.Euler(destination_waypoint.getRotation());
        }
        startRotateCoroutine(final_rotation);
        coroutine_move = null;
        setCharacterIdle();
    }

    private void startRotateCoroutine(Quaternion rotation)
    {
        if (coroutine_rotate != null)
        {
            StopCoroutine(coroutine_rotate);
        }
        coroutine_rotate = RotateCoroutine(rotation);
        StartCoroutine(coroutine_rotate);
    }

    private IEnumerator RotateCoroutine(Quaternion rotation)
    {
        while (transform.rotation != rotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * 500);
            refreshLookAts(rotation.eulerAngles);
            yield return null;
        }
        coroutine_rotate = null;
    }

}
