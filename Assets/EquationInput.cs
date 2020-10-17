using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EquationInput : MonoBehaviour {

    int currentChar = -1;
    int invisChars = 0;
    DateTime dt = DateTime.Now;
    Stack<string> textModifierStack = new Stack<string>();

    public TMP_Text inputField;
    public GameObject equationPanel;
    public GameObject cursor;

    public void GetInput() {
        GraphData.equationString = inputField.text; //turn inputfieldtext into equationsolver syntax

        Graph graph = new Graph();
        graph.GenerateMesh();
    }

    void Update() {
        if (currentChar != -1) {
            if (inputField.textInfo.characterInfo[currentChar].character.ToString() != "") {
                cursor.transform.localPosition = 
                    new Vector3(inputField.textInfo.characterInfo[currentChar].vertex_BR.position.x, 
                    cursor.transform.localPosition.y, 0.0f);
            }
        }
        else if (currentChar == -1 && inputField.text.Length > 0) {
            cursor.transform.localPosition = new Vector3(inputField.textInfo.characterInfo[0].vertex_BL.position.x, cursor.transform.localPosition.y, 0.0f);
        }

        if ((DateTime.Now-dt).TotalSeconds > 0.53) {
            cursor.GetComponent<Image>().color = Color.white;
        }
        if ((DateTime.Now-dt).TotalSeconds > 1.06) {
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
            if (textModifierStack.Count > 0)
            {
                string textModifier = textModifierStack.Pop();
                inputField.text = inputField.text.Substring(0, currentChar + 1 + invisChars) + textModifier + inputField.text.Substring(currentChar + 1 + invisChars);
                invisChars += textModifier.Length;
            }
            else if (currentChar != inputField.text.Length-1) {
                currentChar++;
            }
            if (inputField.text[currentChar + invisChars] == '<')
            {
                while (inputField.text[currentChar + invisChars] != '>') invisChars++;
                invisChars++;
            }

        }
        else if (name == "leftArrow") {
            if (currentChar != -1) 
                currentChar--;
            
            if (inputField.text[currentChar+invisChars] == '>')
            {
                while (inputField.text[currentChar + invisChars] != '<') invisChars--;
                invisChars--;
            }
                
        }
        else if (name == "power") {
            if (currentChar != -1) {
                inputField.text = inputField.text.Substring(0,currentChar+1+invisChars) + "<sup>" + inputField.text.Substring(currentChar+1+invisChars);
            }
            invisChars += 5;
            textModifierStack.Push("</sup>");
        }
        else if (name == "log")
        {
            if (currentChar != -1)
            {
                inputField.text = inputField.text.Substring(0, currentChar + 1 + invisChars) + "log<sub>" + inputField.text.Substring(currentChar + 1 + invisChars);
            } else
            {
                inputField.text = "log<sub>" + inputField.text;
            }
            invisChars += 5;
            currentChar += 3;
            textModifierStack.Push("</sub>");
        }

        else {
            if (currentChar != -1) {
                inputField.text = inputField.text.Substring(0,currentChar+1+invisChars) + name + inputField.text.Substring(currentChar+1+invisChars);
            }
            else {
                inputField.text = name + inputField.text;
            }
            currentChar += name.Length;
            TMP_CharacterInfo[] charinfo = inputField.textInfo.characterInfo;
            Debug.Log(charinfo.Length);
            
        }
    }
}
