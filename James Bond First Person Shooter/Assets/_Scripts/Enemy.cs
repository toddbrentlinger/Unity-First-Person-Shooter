using UnityEngine;

public class Enemy : MonoBehaviour {

    /* NOTES:
     * X If bullet hits enemy, they fall back at the same angle the bullet hits from
     * - Explosion affects enemy by damaging them, killing them if too close, and throwing their body in relation to the explosion 
     * - Enemy skin changes color every time it gets hit or add blood splatter/bullethole decal instead of normal bulletHole decal
     * - Raycast behind enemy to place blood splatter on objects if within a certain distance
     * - Change color of crosshair when enemy is hit
     * - The damage dealt to the target is dependent on several factors, including the weapon's base damage, the damage dropoff, the hitbox that is hit, effects of bullet penetration, effects of armor penetration
     */

    public int currentHealth = 25;

    private bool isAlive = true;

    private Rigidbody enemyRigidbody;

	void Awake () {
        enemyRigidbody = GetComponent<Rigidbody>();
	}

    public void Damage(int damageAmount, Vector3 forceVector, Vector3 forcePosition)
    {
        // Decrease currentHealth by damageAmount
        currentHealth -= damageAmount;

        // If currentHealth is above zero, return
        if (currentHealth > 0)
            return;

        // Enemy currentHealth is zero or less

        // Set isAlive to false
        isAlive = false;

        // Set rigidbody to NOT isKinematic
        enemyRigidbody.isKinematic = false;

        // Add impulse force to enemy
        enemyRigidbody.AddForceAtPosition(forceVector, forcePosition, ForceMode.Impulse);
    }
}
