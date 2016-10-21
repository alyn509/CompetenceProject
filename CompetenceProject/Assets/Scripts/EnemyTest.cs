using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyTest : MonoBehaviour {

    public float moveSpeed = 6.0F;
    public float rotateSpeed = 100.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 rotateDirection = Vector3.zero;
    private NavMeshAgent agent;
    public List<Transform> patrolPoints = new List<Transform>();
    int currentPoint = -1;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        agent.autoBraking = false;

        currentPoint = 0;

        GotoNextPoint();
    }

    void Update()
    {
        // Choose the next destination point when the agent gets
        // close to the current one.
        agent.SetDestination(patrolPoints[currentPoint].position);
        if (agent.remainingDistance < 0.5f)
            GotoNextPoint();

    }

    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (patrolPoints.Count == 0)
            return;

        // Set the agent to go to the currently selected destination.
        agent.destination = patrolPoints[currentPoint].position;
        agent.SetDestination(patrolPoints[currentPoint].position);

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        currentPoint = (currentPoint + 1) % patrolPoints.Count;
    }

}
