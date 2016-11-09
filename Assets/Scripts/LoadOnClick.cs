using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadOnClick : MonoBehaviour {

    public void LoadScene(string level)
    {
        SceneManager.LoadScene(level);
    }
}
