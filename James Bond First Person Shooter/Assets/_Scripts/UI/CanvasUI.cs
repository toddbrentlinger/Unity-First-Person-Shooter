using UnityEngine;
using UnityEngine.UI;

public enum Crosshair { Default, Weapon, Grab };

public class CanvasUI : MonoBehaviour
{
    public static CanvasUI sharedInstance;

    public Transform defaultCrosshair;
    public Transform weaponCrosshair;
    public Text ammoCountText;
    public Text enemyCountText;

    private void Awake()
    {
        // Initialize public static reference to this script instance
        sharedInstance = this;
    }

    private void Start()
    {
        // Update enemy count in Start() after every enemy increments static variable from Awake().
        // This ensures every enemy is accounted for when level loads iniitially.
        UpdateEnemyCount();
    }

    // Toggle weapon crosshair
    public void SetActiveWeaponCrosshair(Crosshair crosshair)
    {
        switch (crosshair)
        {
            case (Crosshair.Weapon):
                // Deactivate defaultCrosshair
                if (defaultCrosshair)
                    defaultCrosshair.gameObject.SetActive(false);

                // Activate weaponCrosshair
                if (weaponCrosshair)
                    weaponCrosshair.gameObject.SetActive(true);

                // Activate ammoCount
                if (ammoCountText)
                    ammoCountText.gameObject.SetActive(true);

                break;

            case (Crosshair.Default):
                // Deactivate ammoCount
                if (ammoCountText)
                    ammoCountText.gameObject.SetActive(false);

                // Deactivate weaponCrosshair
                if (weaponCrosshair)
                    weaponCrosshair.gameObject.SetActive(false);

                // Activate defaultCrosshair
                if (defaultCrosshair)
                    defaultCrosshair.gameObject.SetActive(true);

                break;

            case (Crosshair.Grab):
                break;
        }
        /*
        if (activate)
        {
            // Deactivate defaultCrosshair
            if(defaultCrosshair)
                defaultCrosshair.gameObject.SetActive(false);

            // Activate weaponCrosshair
            if (weaponCrosshair)
                weaponCrosshair.gameObject.SetActive(true);

            // Activate ammoCount
            if (ammoCountText)
                ammoCountText.gameObject.SetActive(true);

        }
        else
        {
            // Deactivate ammoCount
            if (ammoCountText)
                ammoCountText.gameObject.SetActive(false);

            // Deactivate weaponCrosshair
            if (weaponCrosshair)
                weaponCrosshair.gameObject.SetActive(false);

            // Activate defaultCrosshair
            if (defaultCrosshair)
                defaultCrosshair.gameObject.SetActive(true);
        }
        */
    }

    // Ammo Count

    public void UpdateAmmoCount(int ammoInClip, int clipSize)
    {
        if (ammoCountText)
            ammoCountText.text = ammoInClip + " / " + clipSize;
    }

    public void UpdateEnemyCount()
    {
        if (enemyCountText)
            enemyCountText.text = "Enemies: " + Enemy.enemyCount;
    }
}
