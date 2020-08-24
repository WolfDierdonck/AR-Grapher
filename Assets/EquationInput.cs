using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EquationInput : MonoBehaviour {

    public GameObject inputField;

    public void GetInputs() {
        GraphData.equationString = inputField.GetComponent<Text>().text;

        SceneManager.LoadScene(sceneName: "Main");
    }

    public void LoadInput() {
        SceneManager.LoadScene(sceneName: "StartScreen");
    }

}
