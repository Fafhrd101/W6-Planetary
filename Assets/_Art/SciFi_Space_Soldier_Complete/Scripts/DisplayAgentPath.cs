using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class DisplayAgentPath : MonoBehaviour
{
    public bool visualize = false;
    private LineRenderer line; //to hold the line Renderer
    public Transform target; //to hold the transform of the target
    private NavMeshAgent agent; //to hold the agent of this gameObject

    public void Start(){
        line = GetComponent<LineRenderer>(); //get the line renderer
        agent = GetComponent<NavMeshAgent>(); //get the agent
        getPath();
    }

    private void getPath(){

    }

    private void Update()
    {
        if (agent != null && agent.path != null && visualize)
            DrawPath(agent.path);
    }

    private void DrawPath( NavMeshPath path){
        if(path.corners.Length < 2) //if the path has 1 or no corners, there is no need
            return;

        line.positionCount = path.corners.Length; //set the array of positions to the amount of corners

        for(var i = 1; i < path.corners.Length; i++){
            line.SetPosition(i, path.corners[i]); //go through each corner and set that to the line renderer's position
        }
    }
}
