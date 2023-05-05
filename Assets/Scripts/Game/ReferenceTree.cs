using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceTree
{
    public List<string> goal_chains;
    public List<string> goals;
    public List<string> objectives;
    public List<string> scenarios;
    public List<string> hub_npcs;
    public List<string> interactions;
    public List<string> dialogues;
    public List<string> projects;
    public List<string> stations;
    public List<string> script_events;
    public ReferenceTree()
    {
        goal_chains = new List<string>();
        goals = new List<string>();
        objectives = new List<string>();
        scenarios = new List<string>();
        hub_npcs = new List<string>();
        interactions = new List<string>();
        dialogues = new List<string>();
        projects = new List<string>();
        stations = new List<string>();
        script_events = new List<string>();
    }
}
