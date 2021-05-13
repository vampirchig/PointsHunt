using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControls : MonoBehaviour
{
    public void PlayPressed()
    {
        SceneManager.LoadScene("Game");
        Cursor.visible = false;
    }

    public void ToggleSound()
    {
        if (AudioListener.volume==0f)
            AudioListener.volume = 1f;
        else if (AudioListener.volume==1f)
            AudioListener.volume = 0f;
    }

    public void ExitPressed()
    {
        Application.Quit();
    }
}