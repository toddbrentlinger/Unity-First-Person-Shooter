using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuScript : MonoBehaviour {

    [SerializeField] private Canvas m_quitMenu;
    [SerializeField] private Button m_startText;
    [SerializeField] private Button m_exitText;

    // Use this for initialization
    private void Awake()
    {
        m_quitMenu = m_quitMenu.GetComponent<Canvas>();
        m_startText = m_startText.GetComponent<Button>();
        m_exitText = m_exitText.GetComponent<Button>();

        m_quitMenu.enabled = false;
    }

    public void ExitPress()
    {
        m_quitMenu.enabled = true;
        m_startText.enabled = false;
        m_exitText.enabled = false;
    }

    public void NoPress()
    {
        m_quitMenu.enabled = false;
        m_startText.enabled = true;
        m_exitText.enabled = true;
    }

    public void StartLevel()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
