using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {

    /* NOTES:
     * X If bullet hits enemy, they fall back at the same angle the bullet hits from
     * - Explosion affects enemy by damaging them, killing them if too close, and throwing their body in relation to the explosion 
     * - Enemy skin changes color every time it gets hit or add blood splatter/bullethole decal instead of normal bulletHole decal
     * - Raycast behind enemy to place blood splatter on objects if within a certain distance
     * - Change color of crosshair when enemy is hit
     * - The damage dealt to the target is dependent on several factors, including the weapon's base damage, the damage dropoff, the hitbox that is hit, effects of bullet penetration, effects of armor penetration
     */

    // Static variable of number of enemies
    public static int enemyCount = 0;
    
    public int currentHealth = 25;
    private bool m_isAlive = true;

    // Navigation (NavMeshAgent)
    public bool isPatrolling = false; // NOTE: Create patrolNav sub-class or struct to hold NavMesh behavior. Creates dropdown menu that can be closed if isPatrolling is true
    public bool randomizePatrol = false;
    public Transform[] patrolPoints;
    private int m_destinationPoint = 0;
    private NavMeshAgent m_enemyNavMeshAgent;

    // Ragdoll
    private Rigidbody m_enemyRigidbody;
    private Rigidbody[] m_limbRigidbodies;

    // Animation
    private Animator m_enemyAnimator;
    /*
    // Increment static enemyCount whenever a new Enemy is instantiated using constructor function
    public Enemy()
    {
        enemyCount++;
    }
    */
	void Awake ()
    {
        m_enemyRigidbody = GetComponent<Rigidbody>();
        m_limbRigidbodies = GetComponentsInChildren<Rigidbody>();
        m_enemyAnimator = GetComponent<Animator>();
        m_enemyNavMeshAgent = GetComponent<NavMeshAgent>();

        // Increment static enemyCount for every existing gameObject component class Enemy
        enemyCount++;
    }

    void Start()
    {
        // Make sure all limb rigidbodies are kinematic
        if (m_limbRigidbodies.Length > 0)
        {
            foreach (Rigidbody rb in m_limbRigidbodies)
                rb.isKinematic = true;
        }

        // Play first animation state (IDLE) at random start point
        if (m_enemyAnimator)
            m_enemyAnimator.Play(0, -1, Random.value);

        if (isPatrolling && m_enemyNavMeshAgent)
        {
            // Disabling auto-braking allows for continuous movement
            // between points (ie, the agent doesn't slow down as it
            // approaches a destination point).
            m_enemyNavMeshAgent.autoBraking = false;

            GoToNextPatrolPoint();
        }
    }

    void Update()
    {
        // Choose the next destination point when the agent gets close to current one
        if (m_isAlive 
            && isPatrolling 
            && !m_enemyNavMeshAgent.pathPending 
            && m_enemyNavMeshAgent.remainingDistance < 0.5f)
            GoToNextPatrolPoint();
    }

    private void GoToNextPatrolPoint()
    {
        // Returns if no points have been set up
        if (patrolPoints.Length == 0)
            return;

        if (randomizePatrol)
        {
            // Pick random patrol point from array
            // excluding first point, which refers to previous patrol point so it 
            // will NOT be randomly chosen next time. Initially set first patrol point at enemy spawn.
            int n = Random.Range(1, patrolPoints.Length);
            m_enemyNavMeshAgent.destination = patrolPoints[n].position;
            // Move chosen patrol point to index 0 so it's NOT picked next time
            Transform temp = patrolPoints[n];
            patrolPoints[n] = patrolPoints[0];
            patrolPoints[0] = temp;
        }
        else
        {
            // Set the enemyNavMeshAgent to the currently selected destination
            m_enemyNavMeshAgent.destination = patrolPoints[m_destinationPoint].position;

            // Choose the next point in the array as the destination
            // cycling to the start if necessary using modulus operator
            m_destinationPoint = (m_destinationPoint + 1) % patrolPoints.Length;
        }
    }

    // Damage enemy. Called from outside script in FixedUpdate
    public void Damage(int damageAmount, Vector3 forceVector, RaycastHit colliderHit)
    {
        // Return if enemy is NOT alive
        if (!m_isAlive)
            return;

        // Decrease currentHealth by damageAmount
        currentHealth -= damageAmount;

        // If new value of currentHealth is above zero, return
        if (currentHealth > 0)
            return;

        // Enemy currentHealth is zero or less

        // Set isAlive to false
        m_isAlive = false;

        // Decrement static enemyCount and update GUI
        enemyCount--;
        CanvasUI.sharedInstance.UpdateEnemyCount();

        // Disable NavMeshAgent
        if (m_enemyNavMeshAgent)
            m_enemyNavMeshAgent.enabled = false;

        // Set rigidbody to NOT isKinematic
        if (m_enemyRigidbody)
            m_enemyRigidbody.isKinematic = false;
        if (m_limbRigidbodies.Length > 0)
        {
            foreach (Rigidbody rb in m_limbRigidbodies)
            {
                rb.isKinematic = false;
                if (m_enemyAnimator)
                    m_enemyAnimator.enabled = false;
            }
        }

        // Add impulse force to enemy
        colliderHit.rigidbody.AddForceAtPosition(forceVector, colliderHit.point, ForceMode.Impulse);
    }

    private void OnDisable()
    {
        // If enemy is still alive, decrement static variable enemyCount
        if (m_isAlive)
            enemyCount--;
    }
}
