using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuScreen : MonoBehaviour
{
    public static bool gameIsPaused = false;
    [SerializeField] private GameObject m_pauseMenuUI;
    private FPSController m_playerController;

    private void Awake()
    {
        m_playerController = GameObject.FindWithTag("Player").GetComponent<FPSController>();
    }
	
	// Update is called once per frame
	private void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
                Resume();
            else
                Pause();
        }
	}

    public void Resume()
    {
        // Unlock player control
        //m_playerController.playerControl = true;
        if (m_playerController != null)
            m_playerController.enabled = true;

        // Lock cursor and make invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        m_pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    private void Pause()
    {
        // Lock player control
        //m_playerController.playerControl = false;
        if (m_playerController != null)
            m_playerController.enabled = false;

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

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int nextLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextLevelIndex <= SceneManager.sceneCount)
            SceneManager.LoadScene(nextLevelIndex);
    }

    public void PrevLevel()
    {
        Time.timeScale = 1f;
        int prevLevelIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (prevLevelIndex >= 1)
            SceneManager.LoadScene(prevLevelIndex);
    }
}
