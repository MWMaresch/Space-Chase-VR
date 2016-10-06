/*
 *  Class:              GameStateManager
 *  Description:        This manages the scenes/states in the game
 *  Authors:            George Savchenko
 *  Revision History:   
 *  Name:           Date:        Description:
 *  -----------------------------------------------------------------
 */
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    private static GameStateManager instance;
    private string currentScene; // Currenty active scene

    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists and set it to this
        if (instance != null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject); // Make sure there's only one instance of GameStateManager

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        //Call the InitGame function to initialize the first scene
        InitGame();
    }

    // Sets the instance to null when the application quits
    public void OnApplicationQuit()
    {
        instance = null;
    }

    public void InitGame()
    {
        Debug.Log("Starting New State");

        // in order for you to load your scene you must add it to the File > Build Settings in Unity
        currentScene = "scene_menu"; 
        SceneManager.LoadScene("scene_menu");
    }

    // Getter/Setter
    public string getLevel()
    {
        return currentScene;
    }

    public void setLevel(string newScene)
    {
        currentScene = newScene;
    }
}