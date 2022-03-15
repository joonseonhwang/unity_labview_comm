using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class counttime : MonoBehaviour
{
    public static float timeRemaining = 0;
    public Text timeText;
    private void Start()
    {

    }
    void Update()
    {

                timeRemaining += Time.deltaTime;
                timeText.text = timeRemaining.ToString();
        
    }
}