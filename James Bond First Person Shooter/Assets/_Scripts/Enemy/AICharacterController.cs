using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
[RequireComponent(typeof(ThirdPersonCharacter))]
public class AICharacterController : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
    public ThirdPersonCharacter character { get; private set; } // the character we are controlling
    //public Transform target;                                    // target to aim for

    public Transform[] patrolPoints;

    private void Start()
    {
        // get the components on the object we need ( should not be null due to require component so no need to check )
        agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        character = GetComponent<ThirdPersonCharacter>();

        agent.updateRotation = false;
        agent.updatePosition = true;

        SetNextPatrolPoint();
    }


    private void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < .5f)
            SetNextPatrolPoint();

        character.Move(agent.desiredVelocity, false, false);

        /*
        if (target != null)
            agent.SetDestination(target.position);

        if (agent.remainingDistance > agent.stoppingDistance)
            character.Move(agent.desiredVelocity, false, false);
        else
            character.Move(Vector3.zero, false, false);
        */
    }

    private void SetNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
            return;

        int n = Random.Range(1, patrolPoints.Length);
        agent.destination = patrolPoints[n].position;
        // Move chosen patrol point to index 0 so it's NOT picked next time
        Transform temp = patrolPoints[n];
        patrolPoints[n] = patrolPoints[0];
        patrolPoints[0] = temp;
    }
    /*
    public void SetTarget(Transform target)
    {
        this.target = target;
    }
    */
}
