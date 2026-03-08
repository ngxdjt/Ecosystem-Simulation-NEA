using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void QuitGame()
    {
        // Closes application
        Debug.Log("Quit");
        Application.Quit();
    }
}

