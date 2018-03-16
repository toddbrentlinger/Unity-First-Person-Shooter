using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponsController : MonoBehaviour {

    /* NOTES:
     * - Use scroll wheel to scroll between each weapon available, as well as individual number buttons
     * - Weapon Wheel
     */

    public int currentWeapon = -1; // Selected weapon represented by child object index

    // private int selectedWeapon; // Reference to index of previous weapon, to compare to new weapon selection, initialized to int less than zero reprenting NO weapon

    // Use this for initialization
    void Start()
    {
        // Equip initial weapon, if any
        SelectWeapon(currentWeapon);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && transform.childCount >= 1)
            SelectWeapon(0);

        if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
            SelectWeapon(1);

        if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3)
            SelectWeapon(2);

        if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount >= 4)
            SelectWeapon(3);

        if (Input.GetKeyDown(KeyCode.Alpha5) && transform.childCount >= 5)
            SelectWeapon(4);

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (currentWeapon >= transform.childCount - 1)
                SelectWeapon(0);
            else
                SelectWeapon(currentWeapon + 1);
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (currentWeapon <= 0)
                SelectWeapon(transform.childCount - 1);
            else
                SelectWeapon(currentWeapon - 1);
        }
    }

    void SelectWeapon(int selectedWeapon)
    {
        // If there is no selectedWeapon available, return
        if (selectedWeapon >= transform.childCount)
            return;

        // If selectedWeapon is same as currentWeapon OR less than zero, holster weapon, return
        if (selectedWeapon == currentWeapon || selectedWeapon < 0)
        {
            // Disable weapon crosshair, which activates default crosshair
            CanvasUI.sharedInstance.SetActiveWeaponCrosshair(false);

            // Deactivate all weapons
            foreach (Transform weapon in transform)
                weapon.gameObject.SetActive(false);

            // Set currentWeapon to no weapon
            currentWeapon = -1;

            return;
        }

        // If current weapon is negative (holstered), activate weapon cursor, which deactivates default cursor
        if (currentWeapon < 0)
            CanvasUI.sharedInstance.SetActiveWeaponCrosshair(true);

        /// Deactivate all weapons except selectedWeapon
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
            i++;
        }

        // Set currentWeapon to new selectedWeapon
        currentWeapon = selectedWeapon;
    }
}
