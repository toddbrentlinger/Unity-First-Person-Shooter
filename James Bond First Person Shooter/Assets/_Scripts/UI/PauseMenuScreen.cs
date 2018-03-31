using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuScreen : MonoBehaviour {

    public static bool gameIsPaused = false;
    [SerializeField] private GameObject m_pauseMenuUI;
	
	// Update is called once per frame
	private void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
                
            else
            {
                Pause();
            }
                
        }	
	}

    public void Resume()
    {
        // Lock cursor and make invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        m_pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    private void Pause()
    {
        // Unlock cursor and make visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        m_pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
