using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EquationInput : MonoBehaviour {

    public TMP_Text inputField;

    public void GetInput() {
        GraphData.equationString = inputField.text;

        Graph graph = new Graph();
        graph.GenerateMesh();
    }

    public void ChangeInput(string name) {
        if (name != "backspace") {
            inputField.text = inputField.text+name;
        }
        else {
            inputField.text = inputField.text.Remove(inputField.text.Length - 1);
        }
    }
}
