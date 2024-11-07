using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenderToggle : MonoBehaviour
{

    public Toggle toogle;
    public Text gender;
    // Start is called before the first frame update
    void Start()
    {    
        toogle.onValueChanged.AddListener(delegate
        {
            if (toogle.isOn)
            {
                gender.color = new Color(1, 1, 1);
            }
            else
            {
                gender.color = new Color(0, 0, 0);
            }        
        });
    }
}
