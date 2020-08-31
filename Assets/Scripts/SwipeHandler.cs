using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeHandler : MonoBehaviour {
    public static bool swiped = false;
    public static Vector3 touchDown = Vector3.zero;
    public static Vector3 touchUp = Vector3.zero;
    private bool touching;
    private float touchTimer = 0;
    private float swipeCooldown = 0;

    void Update() {
        if (touching) touchTimer += Time.deltaTime;
        if (!swiped && Input.GetMouseButtonDown(0)) {
            touchDown = Input.mousePosition;
            touching = true;
        }
        if (touching && Input.GetMouseButtonUp(0)) {
            touchUp = Input.mousePosition;
            CheckSwipe();
            touching = false;
            touchTimer = 0;
        }

        if (swiped && Time.time >= swipeCooldown) {
            swiped = false;
        }
    }

    private void CheckSwipe() {
        if (touchTimer < .1f) return;
        if (Vector3.Distance(touchUp, touchDown)  < .025f*Screen.width) return;

        swiped = true;
        swipeCooldown = Time.time + .25f;
    }
}
