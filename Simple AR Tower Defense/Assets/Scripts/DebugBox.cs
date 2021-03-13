using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugBox : MonoBehaviour
{

    [SerializeField]
    Text debugDisplay;

    public void Print(object s) {
        debugDisplay.text += s.ToString();
    }

    public void PrintLine(object s) {
        debugDisplay.text += "\n";
        debugDisplay.text += s.ToString();
    }

    public void Clear() {
        debugDisplay.text = "";
    }
}
