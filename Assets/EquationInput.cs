using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class EquationInput : MonoBehaviour {

    string usableString = "";
    string latexString = "";
    bool settingsOpen = false;
    int currentChar = -1;

    public GameObject equationField;
    public GameObject equationPanel;
    public GameObject cursor;
    public GameObject settingsPanel;
    public GameObject detailSlider;
    public GameObject boundsSlider;
    public GameObject zButton;


    public void GetInput() {
        usableString = latexString;

        usableString = usableString.Replace("\\", "");

        foreach (Match match in Regex.Matches(usableString, "sqrt"))
        {
            int startIndex = match.Index + 4;
            char[] array = usableString.ToCharArray();
            array[startIndex] = '(';
            while (usableString[startIndex] != ']') startIndex++;
            array[startIndex] = ',';
            array[startIndex + 1] = ' ';
            usableString = new string(array);
        }

        foreach (Match match in Regex.Matches(usableString, "log"))
        {
            int startIndex = match.Index + 5;
            char[] array = usableString.ToCharArray();
            array[startIndex - 1] = '(';
            int bracketCounter = 1;
            while (bracketCounter != 0) {
                if (array[startIndex] == '{') bracketCounter++;
                else if (array[startIndex] == '}') bracketCounter--; 
                startIndex++;
            }
          
            array[startIndex-1] = ',';
            array[startIndex] = ' ';
            usableString = new string(array);
        }

        char[] yeet = usableString.ToCharArray();
        for (int i = 0; i < yeet.Length; i++)
        {
            if (yeet[i] == '{') yeet[i] = '(';
            if (yeet[i] == '}') yeet[i] = ')';
            if (yeet[i] == '\\') yeet[i] = ' ';
            if (yeet[i] == '_') yeet[i] = ' ';
        }

        usableString = new string(yeet);
        Debug.Log(usableString);

        GraphData.equationString = usableString; //turn inputfieldtext into equationsolver syntax

        Graph graph = new Graph();
        graph.GenerateMesh();
    }


    public void SettingsClicked()
    {
        settingsPanel.SetActive(!settingsOpen);
        settingsOpen = !settingsOpen;
    }

    public void ChangeInput(string name) {
        
        if (name == "backspace") {
            int startingChar;
            if (currentChar != -1) {
                while (latexString[currentChar] == '[' || latexString[currentChar] == ']' || latexString[currentChar] == '{' || latexString[currentChar] == '}' || latexString[currentChar] == '\\') currentChar--;

                startingChar = currentChar;
                while (((int)latexString[currentChar] >= (int)'a' && (int)latexString[currentChar] < (int)'x'))
                {
                    if (currentChar == 0) break;
                    currentChar--;
                }

                latexString = latexString.Remove(currentChar, startingChar-currentChar+1);
                if (currentChar != -1) currentChar--;
            }
            latexString = latexString.Replace("[]", "");
        }
        else if (name == "rightArrow") {
            if (currentChar != latexString.Length - 1) {
                currentChar++;
                if (latexString[currentChar] == ']')
                {
                    currentChar++;
                }
                    
            }

        }
        else if (name == "leftArrow") {
            if (currentChar != -1)
            {
                currentChar--;

            }

        }
        else if (name == "power") {
            if (currentChar != -1)
            {
                latexString = latexString.Substring(0, currentChar + 1) + "^{}" + latexString.Substring(currentChar + 1);
            }
            else
            {
                latexString = "" + latexString;
            }
            currentChar += 2;
        }
        else if (name == "log")
        {
            if (currentChar != -1)
            {
                latexString = latexString.Substring(0, currentChar + 1) + "log_{}{}" + latexString.Substring(currentChar + 1);
            }
            else
            {
                latexString = "log_{}{}" + latexString;
            }
            currentChar += 5;
        }
        else if (name == "sqrt")
        {
            if (currentChar != -1)
            {
                latexString = latexString.Substring(0, currentChar + 1) + "\\sqrt[2]{}" + latexString.Substring(currentChar + 1);
            }
            else
            {
                latexString = "\\sqrt[2]{}" + latexString;
            }
            currentChar += 9;
        }
        else if (name == "root")
        {
            if (currentChar != -1)
            {
                latexString = latexString.Substring(0, currentChar + 1) + "\\sqrt[]{}" + latexString.Substring(currentChar + 1);
            }
            else
            {
                latexString = "\\sqrt[]{}" + latexString;
            }
            currentChar += 6;
        }
        else if (name == "log10")
        {
            if (currentChar != -1)
            {
                latexString = latexString.Substring(0, currentChar + 1) + "log_{10}{}" + latexString.Substring(currentChar + 1);
            }
            else
            {
                latexString = "log_{10}{}" + latexString;
            }
            currentChar += 9;
        }

        else {
            if (currentChar != -1) {
                latexString = latexString.Substring(0, currentChar + 1) + name + latexString.Substring(currentChar + 1);
            }
            else {
                latexString = name + latexString;
            }
            currentChar += name.Length;

        }

        equationField.GetComponent<TEXDraw>().text = "$" + latexString + "$";
        if (latexString.Contains("z"))
        {
            zButton.GetComponent<Button>().enabled = false;
        } else
        {
            zButton.GetComponent<Button>().enabled = true;
        }

    }

    public void BoundsChanged()
    {
        GraphData.boundMaxX = (int) boundsSlider.GetComponent<Slider>().value * 5;
        GraphData.boundMinX = (int)boundsSlider.GetComponent<Slider>().value * -5;
        GraphData.boundMaxY = (int)boundsSlider.GetComponent<Slider>().value * 5;
        GraphData.boundMinY = (int)boundsSlider.GetComponent<Slider>().value * -5;
    }

    public void DetailChanged()
    {
        GraphData.xSize = (int)detailSlider.GetComponent<Slider>().value * 50;
        GraphData.zSize = (int)detailSlider.GetComponent<Slider>().value * 50;
    }
}
