using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SwipeCounter : MonoBehaviour
{
    public TextMeshProUGUI swipes;
    public TextMeshProUGUI target;

    private int numSwipes = 0;

    public void AddSwipe() {
        numSwipes++;
        swipes.text = ""+numSwipes;
    }

    public void SetTarget(int targetSwipes) {
        target.text = ""+targetSwipes;
    }

    public void ResetSwipes() {
        numSwipes = 0;
        swipes.text = ""+0;
    }
}
