using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextChange : MonoBehaviour
{
    public Text mytext;
    bool toggle;
    public void SetText()
    {
        toggle = !toggle;
        if (!toggle)
            mytext.text = "звук включен";
        else
            mytext.text = "звук выключен";
    }
}
 