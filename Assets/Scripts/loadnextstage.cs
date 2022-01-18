using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadnextstage : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene(load_next.NextLevel);
    }
}
