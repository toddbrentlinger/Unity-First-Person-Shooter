using UnityEngine;
using UnityEngine.UI;

public class CanvasUI : MonoBehaviour {

    public static CanvasUI sharedInstance;

    public Transform defaultCrosshair;
    public Transform weaponCrosshair;
    public Text ammoCountText;

    private void Awake()
    {
        // Initialize public static referene to this script instance
        sharedInstance = this;
    }

    // Toggle weapon crosshair
    public void SetActiveWeaponCrosshair(bool activate = true)
    {
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
    }

    // Ammo Count

    public void UpdateAmmoCount(int ammoInClip, int clipSize)
    {
        if (ammoCountText)
            ammoCountText.text = ammoInClip + " / " + clipSize;
    }
}
