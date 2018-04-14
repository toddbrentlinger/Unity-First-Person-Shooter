using UnityEngine;

public class PlayerWeaponsController : MonoBehaviour {

    /* NOTES:
     * - Use scroll wheel to scroll between each weapon available, as well as individual number buttons
     * - Weapon Wheel
     * - Use currentWeapon <= 0 as noWeapon/melee instead of -1 so noWeapon is option when scrolling mousewheel instead of having to press specific weapon key to unequip weapon
     * - Can only grab objects if no weapon is selected
     * - Before switching weapon, play exit animation state which should drop weapon out of frame to unequip
     */

    public int currentWeapon = 0; // Selected weapon represented by child object index

    // private int selectedWeapon; // Reference to index of previous weapon, to compare to new weapon selection, initialized to int less than zero reprenting NO weapon

    // Use this for initialization
    void Start()
    {
        // Equip initial weapon, if any
        int temp = currentWeapon;
        currentWeapon = 0;
        SelectWeapon(temp);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && transform.childCount > 0)
                SelectWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount > 1)
                SelectWeapon(2);
            if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount > 2)
                SelectWeapon(3);
            if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount > 3)
                SelectWeapon(4);
            if (Input.GetKeyDown(KeyCode.Alpha5) && transform.childCount > 4)
                SelectWeapon(5);
            if (Input.GetKeyDown(KeyCode.Alpha6) && transform.childCount > 5)
                SelectWeapon(6);
            if (Input.GetKeyDown(KeyCode.Alpha7) && transform.childCount > 6)
                SelectWeapon(7);
            if (Input.GetKeyDown(KeyCode.Alpha8) && transform.childCount > 7)
                SelectWeapon(8);
            if (Input.GetKeyDown(KeyCode.Alpha9) && transform.childCount > 8)
                SelectWeapon(9);
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (currentWeapon <= 0)
                SelectWeapon(transform.childCount);
            else
                SelectWeapon(currentWeapon - 1);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (currentWeapon >= transform.childCount)
                SelectWeapon(0);
            else
                SelectWeapon(currentWeapon + 1);
        }
    }

    void SelectWeapon(int selectedWeapon)
    {
        // If selectedWeapon is not available (out of range), return
        if (selectedWeapon > transform.childCount || selectedWeapon < 0)
            return;

        // If selectedWeapon is same as currentWeapon OR zero, holster weapon, return
        if (selectedWeapon == currentWeapon || selectedWeapon == 0)
        {
            // Disable weapon crosshair, which activates default crosshair
            CanvasUI.sharedInstance.SetActiveWeaponCrosshair(false);

            // Deactivate all weapons
            foreach (Transform weapon in transform)
                weapon.gameObject.SetActive(false);

            // Set currentWeapon to no weapon
            currentWeapon = 0;

            return;
        }

        // If current weapon is negative (holstered), activate weapon cursor, which deactivates default cursor
        if (currentWeapon == 0)
            CanvasUI.sharedInstance.SetActiveWeaponCrosshair(true);

        /// Deactivate all weapons except selectedWeapon
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon - 1)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
            i++;
        }

        // Set currentWeapon to new selectedWeapon
        currentWeapon = selectedWeapon;
    }
}
