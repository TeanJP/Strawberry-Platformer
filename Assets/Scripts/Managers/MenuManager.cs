using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class MenuManager : MonoBehaviour
{
    [SerializeField]
    protected GameObject currentScreen = null;

    public void OpenScreen(GameObject newScreen)
    {
        currentScreen.SetActive(false);
        newScreen.SetActive(true);
        currentScreen = newScreen;
    }

    public void LoadScene(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
