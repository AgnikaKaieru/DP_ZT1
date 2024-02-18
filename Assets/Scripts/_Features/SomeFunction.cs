using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Features
{
    public class SomeFunction : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text_FunctionOutput;

        public void ExecuteFunction()
        {
            if (text_FunctionOutput.text != "") { Debug.LogWarning("Text already written."); return; }

            string output = "";
            for (int i = 1; i <= 100; i++)
            {
                int r3 = i % 3;
                int r5 = i % 5;
                if (r3 == 0)
                {
                    if (r5 == 0) output += "MarkoPolo (" + i + ")";
                    else output += "Marko (" + i + ")";
                }
                else if (r5 == 0) output += "Polo (" + i + ")";
                else output += i.ToString();
                output += "\n";
            }
            text_FunctionOutput.text = output;
        }
    }
}
