using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour {

    [SerializeField] GameObject m_loadingScreen;
    [SerializeField] Slider m_loadingSlider;
    [SerializeField] TextMeshProUGUI m_progressText;

    public void PlayGame()
    {
        StartLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartLevel(int sceneIndex)
    {
        if (m_loadingScreen)
            LoadLevel(sceneIndex);
        else
            SceneManager.LoadScene(sceneIndex);
    }

    private void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    private IEnumerator LoadAsynchronously(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        m_loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);

            m_progressText.text = Mathf.CeilToInt(progress * 100) + "%";

            m_loadingSlider.value = progress;

            yield return null;
        }
    }

}
