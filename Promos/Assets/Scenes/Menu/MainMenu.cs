using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class mainMenu : MonoBehaviour
{
    public GameObject MenuUI;
    public GameObject LoadingScream;
    public float loadPlay = 3f;

    public void StartGame()
    {
        if (LoadingScream != null)
        {
            LoadingScream.SetActive(false);
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

    public void QuitGame()
    {
        Application.Quit();
    }
}
