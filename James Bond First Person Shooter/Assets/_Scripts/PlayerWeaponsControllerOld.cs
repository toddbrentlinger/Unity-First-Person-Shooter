using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponsControllerOld : MonoBehaviour {

    /* NOTES:
     * - Use scroll wheel to scroll between each weapon available, as well as individual number buttons
     * - Weapon Wheel
     */

    public Transform[] weapons; // Array of references to weapon transforms
    public Transform defaultCrosshair;
    public Transform weaponCrosshair;

    private Transform currWeapon = null; // Transform of currWeapon active initialized to null reprenting NO weapon active

    // Use this for initialization
    void Start()
    {
        // Check if a weapon is equipped when scene loads
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].gameObject.activeSelf)
            {
                // Set currWeapon to currently equipped weapon
                currWeapon = weapons[i];

                // Deactivate defaultCrosshair
                defaultCrosshair.gameObject.SetActive(false);

                // Activate weaponCrosshair
                weaponCrosshair.gameObject.SetActive(true);

                break;
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown("1")) EquipWeapon(0);
        else if (Input.GetKeyDown("2")) EquipWeapon(1);
        else if (Input.GetKeyDown("3")) EquipWeapon(2);
        else if (Input.GetKeyDown("4")) EquipWeapon(3);
        else if (Input.GetKeyDown("5")) EquipWeapon(4);
        else if (Input.GetKeyDown("6")) EquipWeapon(5);
        else if (Input.GetKeyDown("7")) EquipWeapon(6);
        
    }
    
    // Method: Equip weapon
    void EquipWeapon(int arrayIndex)
    {
        /* NOTES:
         *  Maybe use weapon.activeSelf instead of weapon.activeInHeirarchy
        */

        // If arrayIndex is outside array, end function
        if (arrayIndex >= weapons.Length || arrayIndex < 0) return;

        // If weapon being equipped does NOT exist, end function
        if (weapons[arrayIndex] == null) return;

        // If currWeapon already equipped is the same weapon trying to be equipped, unequip currWeapon
        if (currWeapon == weapons[arrayIndex])
        {
            // Deactivate weaponCrosshair
            weaponCrosshair.gameObject.SetActive(false);

            // Activate defaultCrosshair
            defaultCrosshair.gameObject.SetActive(true);

            // Deactivate weapon GameObject
            currWeapon.gameObject.SetActive(false);

            // Set currWeapon to null representing no weapon equipped
            currWeapon = null;

            // End function
            return;
        }

        // Weapon being equipped exists and is NOT the same weapon currently equipped

        // If currWeapon is null
        if (currWeapon == null)
        {
            // Set currWeapon to selected weapon in array
            currWeapon = weapons[arrayIndex];

            // Activate weapon GameObject
            currWeapon.gameObject.SetActive(true);

            // Deactivate defaultCrosshair
            defaultCrosshair.gameObject.SetActive(false);

            // Activate weaponCrosshair
            weaponCrosshair.gameObject.SetActive(true);

            // End function
            return;
        }

        // Player has another weapon currently equipped

        // Deactivate currentWeapon
        currWeapon.gameObject.SetActive(false);

        // Set currWeapon to selected weapon in array
        currWeapon = weapons[arrayIndex];

        // Activate weapon GameObject
        currWeapon.gameObject.SetActive(true);
    }
}
