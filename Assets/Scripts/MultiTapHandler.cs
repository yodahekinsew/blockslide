using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiTapHandler : MonoBehaviour {
    public static bool tapped = false;
    public static int taps = 0;

    private bool touching = false;
    private float touchTimer = 0;
    private float tapThreshold = .25f;
    private float tapTimer = 0;
    private float tapCooldown = 0;
    void Update() {
        if (!tapped && Input.GetMouseButtonDown(0)) touching = true;
        if (touching && Input.GetMouseButtonUp(0)) {
            taps ++;
            touching = false;
            // tap timer basically checks for the time between taps for a double tap
            tapTimer = Time.time + tapThreshold;
        }
        // If we detect 1 or more taps, trigger the double tap and reset
        if (!tapped && taps > 0 && Time.time >= tapTimer) {
            tapped = true;
            touching = false;
            tapCooldown = Time.time + Time.deltaTime;
        }
        if (tapped && Time.time >= tapCooldown) {
            tapped = false;
            taps = 0;
        }
    }
}
