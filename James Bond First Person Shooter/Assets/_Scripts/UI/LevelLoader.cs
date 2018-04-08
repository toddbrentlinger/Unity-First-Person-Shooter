using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelLoader : MonoBehaviour {

    [SerializeField] private GameObject m_loadingScreen;
    [SerializeField] private Slider m_loadingSlider;
    [SerializeField] private RectTransform m_spinningGunBarrel;
    [SerializeField] private float m_spinAngularSpeed = 50f;
    [SerializeField] private TextMeshProUGUI m_progressText;

    public void LoadLevel(int sceneIndex)
    {
        if (!LevelAvailable(sceneIndex))
            return;
         
        if (m_loadingScreen)
            StartCoroutine(LoadAsynchronously(sceneIndex));
        else
            SceneManager.LoadScene(sceneIndex);
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

            // Spin gun barrel
            m_spinningGunBarrel.Rotate(Vector3.forward * m_spinAngularSpeed * Time.deltaTime);

            yield return null;
        }
    }
    
    private bool LevelAvailable(int sceneIndex)
    {
        // Return true if sceneIndex is between 1 and sceneCount
        //return !(sceneIndex < 1 || sceneIndex > SceneManager.sceneCount);

        return Application.CanStreamedLevelBeLoaded(sceneIndex);
    }
    
}
