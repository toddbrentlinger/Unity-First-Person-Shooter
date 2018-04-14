using UnityEngine;
using UnityEngine.AI;

/* NOTES:
 * X If bullet hits enemy, they fall back at the same angle the bullet hits from
 * - Explosion affects enemy by damaging them, killing them if too close, and throwing their body in relation to the explosion 
 * - Enemy skin changes color every time it gets hit or add blood splatter/bullethole decal instead of normal bulletHole decal
 * - Raycast behind enemy to place blood splatter on objects if within a certain distance
 * - Change color of crosshair when enemy is hit
 * - The damage dealt to the target is dependent on several factors, including the weapon's base damage, the damage dropoff, the hitbox that is hit, effects of bullet penetration, effects of armor penetration
 */

public class EnemyAdvanced : MonoBehaviour {

    // Static variable of number of enemies
    public static int enemyCount = 0;

    // Enemy State
    public enum EnemyState {Idle, Combat};
    [SerializeField] private EnemyState m_enemyState;

    // General
    public int currentHealth = 25;
    private bool m_isAlive = true;

    // Ragdoll
    private Rigidbody m_enemyRigidbody;

    private void Awake()
    {
        // Increment static enemyCount for every existing gameObject component class Enemy
        enemyCount++;

        m_enemyRigidbody = GetComponent<Rigidbody>();
    }

    private void Start ()
    {
        m_enemyState = EnemyState.Idle;
	}
	
	private void Update ()
    {
        if (!m_isAlive)
            return;

        switch (m_enemyState)
        {
            case (EnemyState.Idle):
                break;

            case (EnemyState.Combat):
                EnemyCombat();
                break;
        }
	}

    // Damage enemy. Called from outside script in FixedUpdate
    public void Damage(int damageAmount, Vector3 forceVector, RaycastHit colliderHit)
    {
        // Return if enemy is NOT alive
        if (!m_isAlive)
            return;

        // If NOT already in combat state, change to combat state
        if (m_enemyState != EnemyState.Combat)
            m_enemyState = EnemyState.Combat;

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

        // Set rigidbody to NOT isKinematic
        if (m_enemyRigidbody)
            m_enemyRigidbody.isKinematic = false;

        // Add impulse force to enemy
        colliderHit.rigidbody.AddForceAtPosition(forceVector, colliderHit.point, ForceMode.Impulse);
    }

    // EnemyState.Combat
    private void EnemyCombat()
    {

    }

    private void OnDisable()
    {
        // If enemy is still alive, decrement static variable enemyCount
        if (m_isAlive)
            enemyCount--;
    }
}
