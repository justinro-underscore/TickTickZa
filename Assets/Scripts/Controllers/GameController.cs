using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public static GameController instance = null;


    public bool gameOver = false;
    private bool isRunning;

    private SceneController.Level currentLevel;

    void Start() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        // Start with main game level
        SceneController.LoadLevel( SceneController.Level.MAIN_LEVEL );
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void Update() {
        switch( SceneController.GetCurrentLevel() ) {
            case SceneController.Level.MAIN_LEVEL:
                RunGame();
                break;
            case SceneController.Level.PAUSE_MENU:
                RunPauseMenu();
                break;
        }
    }

    void RunPauseMenu() {
        if ( Input.GetKeyDown( KeyCode.Escape ) ) {
            ExitPauseMenu();
        }
    }

    void RunGame() {
        if ( Input.GetKeyDown( KeyCode.Escape ) ) {
            SceneController.LoadLevel( SceneController.Level.PAUSE_MENU );
            SceneManager.LoadScene("PauseMenuScene", LoadSceneMode.Additive);

            // Set time to 0 to freeze everything
            Time.timeScale = 0.0f;
        }

        // TODO: remove this - this is just for testing
        if( Input.GetKeyDown(KeyCode.R) )
        {
            List<Constants.Meats> meats = new List<Constants.Meats>();
            meats.Add(Constants.Meats.Pepperoni);

            DeliveryManager.dmInstance.DeliverMeat(meats);
        }
    }

    private void OnSceneUnloaded( Scene unloadedScene ) {
        if ( unloadedScene.name == "PauseMenuScene" ) {
            ExitPauseMenu();
        }
    }

    private void ExitPauseMenu() {
        SceneController.LoadLevel( SceneController.Level.MAIN_LEVEL );
        SceneManager.UnloadSceneAsync("PauseMenuScene");

        // Unfreeze time
        Time.timeScale = 1.0f;
    }
}