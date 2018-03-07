using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponsController : MonoBehaviour {
    
    /* NOTES:
     * - Use scroll wheel to scroll between each weapon available, as well as individual number buttons
     */

    public GameObject weapon; // Reference to weapon GameObject
    public Transform defaultCrosshair;
    public Transform weaponCrosshair;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown("1"))
        {
            // Toggle weapon
            EquipWeapon();
        }
	}

    // Method: Equip weapon
    void EquipWeapon()
    {
        /* NOTES:
         *  Maybe use weapon.activeSelf instead of weapon.activeInHeirarchy
        */

        // If weapon is NOT active
        if (!weapon.activeInHierarchy)
        {
            // Deactivate defaultCrosshair
            defaultCrosshair.gameObject.SetActive(false);

            // Activate weaponCrosshair
            weaponCrosshair.gameObject.SetActive(true);

            // Activate weapon GameObject
            weapon.SetActive(true);
        }
        // Else weapon is active
        else
        {
            // Deactivate weaponCrosshair
            weaponCrosshair.gameObject.SetActive(false);

            // Activate defaultCrosshair
            defaultCrosshair.gameObject.SetActive(true);

            // Deactivate weapon GameObject
            weapon.SetActive(false);
        }
        
    }
}
