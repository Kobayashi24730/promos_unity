using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public GameObject MenuUI;
    public GameObject LoadingScream;
    public GameObject SettingsUI;
    public float loadPlay = 3f;

    // Executado automaticamente assim que a cena começa
    private void Start()
    {
        ResetMenuState();
    }

    // Força o estado inicial limpo do menu
    public void ResetMenuState()
    {
        if (LoadingScream != null)
        {
            LoadingScream.SetActive(false);
        }
        if (SettingsUI != null)
        {
            SettingsUI.SetActive(false);
        }
        if (MenuUI != null)
        {
            MenuUI.SetActive(true);
        }
    }    

    public void OnClickStartGame(string sceneName)
    {
        StartCoroutine(LoadGameRoutine(sceneName));
    }

    private IEnumerator LoadGameRoutine(string sceneName)
    {
        if (MenuUI != null)
        {
            MenuUI.SetActive(false);
        }

        if (LoadingScream != null)
        {
            LoadingScream.SetActive(true);
        }

        yield return new WaitForSeconds(loadPlay);
        SceneManager.LoadScene(sceneName);
    }

    public void OpenSettings()
    {
        if (MenuUI != null) MenuUI.SetActive(false);
        if (LoadingScream != null) LoadingScream.SetActive(false);
        if (SettingsUI != null) SettingsUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (MenuUI != null) MenuUI.SetActive(true);
        if (LoadingScream != null) LoadingScream.SetActive(false);
        if (SettingsUI != null) SettingsUI.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}