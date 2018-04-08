using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(LevelLoader))]
public class MainMenu : MonoBehaviour {
    /*
    [SerializeField] private GameObject m_loadingScreen;
    [SerializeField] private Slider m_loadingSlider;
    [SerializeField] private RectTransform m_spinningGunBarrel;
    [SerializeField] private float m_spinAngularSpeed = 50f;
    [SerializeField] private TextMeshProUGUI m_progressText;
    */

    private LevelLoader m_levelLoader;
    
    private void Awake()
    {
        m_levelLoader = GetComponent<LevelLoader>();

        // Make sure cursor is visible and NOT locked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayGame()
    {
        m_levelLoader.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    /*
    public void StartLevel(int sceneIndex)
    {
        if (m_loadingScreen)
            LoadLevel(sceneIndex);
        else
            SceneManager.LoadScene(sceneIndex);
    }
    */
    public void LoadLevel(int sceneIndex)
    {
        m_levelLoader.LoadLevel(sceneIndex);
    }
    /*
    private IEnumerator LoadAsynchronously(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        m_loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);

            m_progressText.text = Mathf.CeilToInt(progress * 100) + "%";

            m_loadingSlider.value = progress;

            // Spin gun barrel
            m_spinningGunBarrel.Rotate(Vector3.forward * m_spinAngularSpeed * Time.deltaTime);

            yield return null;
        }
    }
    */
}
