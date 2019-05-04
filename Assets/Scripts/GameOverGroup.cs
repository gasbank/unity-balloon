﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverGroup : MonoBehaviour {
    public void ReloadMainScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SelectStage() {
        SceneManager.LoadScene("Stage Selection");
    }
}
