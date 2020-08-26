using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EquationInput : MonoBehaviour {

    int currentChar = -1;
    DateTime dt = DateTime.Now;

    public TMP_Text inputField;
    public GameObject equationPanel;
    public GameObject cursor;

    public void GetInput() {
        GraphData.equationString = inputField.text;

        Graph graph = new Graph();
        graph.GenerateMesh();
    }


    void Update() {
        if (currentChar != -1) {
            if (inputField.textInfo.characterInfo[currentChar].character.ToString() != "") {
                cursor.transform.localPosition = new Vector3(inputField.textInfo.characterInfo[currentChar].vertex_BR.position.x, cursor.transform.localPosition.y, 0.0f);
            }
        }
        else if (currentChar == -1 && inputField.text.Length > 0) {
            cursor.transform.localPosition = cursor.transform.localPosition = new Vector3(inputField.textInfo.characterInfo[0].vertex_BL.position.x, cursor.transform.localPosition.y, 0.0f);
        }

        if ((DateTime.Now-dt).TotalSeconds > 1) {
            cursor.GetComponent<Image>().color = Color.white;
        }
        if ((DateTime.Now-dt).TotalSeconds > 1.3) {
            cursor.GetComponent<Image>().color = Color.black;
            dt = DateTime.Now;
        }
    }

    public void ChangeInput(string name) {
        if (name == "backspace") {
            if (currentChar != -1) {
                inputField.text = inputField.text.Remove(currentChar, 1);
                currentChar--;
            }
        }
        else if (name == "rightArrow") {
            if (currentChar != inputField.text.Length-1) {
                currentChar++;
            }
        }
        else if (name == "leftArrow") {
            if (currentChar != -1) {
                currentChar--;
            }
        }
        else {
            if (currentChar != -1) {
                inputField.text = inputField.text.Substring(0,currentChar+1) + name + inputField.text.Substring(currentChar+1);
            }
            else {
                inputField.text = name + inputField.text;
            }
            currentChar = currentChar + name.Length;
        }
    }
}
