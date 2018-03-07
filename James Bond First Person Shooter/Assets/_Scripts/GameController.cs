using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    bool paused = false;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        // Input handling
        if (Input.GetKeyDown("p"))
        {
            if (!paused)
            {
                Pause();
                paused = true;
            }
            else
            {
                Resume();
                paused = false;
            }
        }
    }

    // Pause Game
    void Pause()
    {
        Time.timeScale = 0;
    }

    // Resume Game
    void Resume()
    {
        Time.timeScale = 1;
    }
}
