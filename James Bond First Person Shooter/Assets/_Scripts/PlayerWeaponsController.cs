using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponsController : MonoBehaviour {
    
    /* NOTES:
     * - Use scroll wheel to scroll between each weapon available, as well as individual number buttons
     */

    public Transform[] weapons; // Array of references to weapon transforms
    public Transform defaultCrosshair;
    public Transform weaponCrosshair;

    private Transform currWeapon = null;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("1"))
        {
            // Toggle weapon
            EquipWeapon(0);
        }
        else if (Input.GetKeyDown("2")) EquipWeapon(1);
	}

    // Method: Equip weapon
    void EquipWeapon(int arrayIndex)
    {
        /* NOTES:
         *  Maybe use weapon.activeSelf instead of weapon.activeInHeirarchy
        */

        // If currWeapon already active is weapon being equipped, end function
        // if (currWeapon == weapons[arrayIndex]) return;

        // Get new currWeapon reference to weapon in array
        currWeapon = weapons[arrayIndex];

        // If there is no weapon in array index, end function
        if (currWeapon == null) return;

        // If weapon is NOT active in heirarchy, equip weapon
        if (!currWeapon.gameObject.activeInHierarchy)
        {
            // Deactivate defaultCrosshair
            defaultCrosshair.gameObject.SetActive(false);

            // Activate weaponCrosshair
            weaponCrosshair.gameObject.SetActive(true);

            // Activate weapon GameObject
            currWeapon.gameObject.SetActive(true);
        }
        // Else weapon is active in heirarchy, unequip weapon
        else
        {
            // Deactivate weaponCrosshair
            weaponCrosshair.gameObject.SetActive(false);

            // Activate defaultCrosshair
            defaultCrosshair.gameObject.SetActive(true);

            // Deactivate weapon GameObject
            currWeapon.gameObject.SetActive(false);
        }
        
    }
}
