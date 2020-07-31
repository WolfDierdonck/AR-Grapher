using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EquationInput : MonoBehaviour {
    public string equationString;
    public int boundMinX;
    public int boundMaxX;
    public int boundMinY;
    public int boundMaxY;

    public GameObject inputField;
    public GameObject minXField;
    public GameObject maxXField;
    public GameObject minYField;
    public GameObject maxYField;
    
    public void GetInputs() {
        GraphData.equationString = inputField.GetComponent<Text>().text;

        SceneManager.LoadScene(sceneName: "Main");
    }

}
