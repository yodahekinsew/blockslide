using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public enum GameState {
    Starting,
    Playing,
    Reset,
    Finished,
    Paused
}

public class GameStateManager : MonoBehaviour
{
    public static GameState state = GameState.Starting;

    public GameGrid grid;
    public Transform blocks;

    // Level handling
    private int currentLevel = 1;
    private int numLevels = 0;

    // Resetting
    private float resetCooldown = 0;

    // Starting Screen
    private bool started = false;
    public Transform startingGrid;

    // UI handling
    public GameObject startingUI;
    public GameObject playingUI;
    public SwipeCounter swipeCounter;
    public TextMeshProUGUI levelCounter;

    private delegate void Func();
    
    void Start()
    {
        numLevels = SceneManager.sceneCountInBuildSettings - 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == GameState.Starting) {
            if (!started && SwipeHandler.swiped) {
                Vector3 swipe = SwipeHandler.touchUp - SwipeHandler.touchDown;
                if (swipe.y > swipe.x && swipe.y > 0) {
                    StartPlaying();
                    started = true;
                }
            }
            return;
        }

        if (Time.time >= resetCooldown && MultiTapHandler.tapped && MultiTapHandler.taps >= 2) {
            ResetLevel();
            resetCooldown = Time.time + .5f;
        }
    }

    public void ResetLevel()
    {
        state = GameState.Reset;
        swipeCounter.ResetSwipes();
        grid.Reset();
        foreach(Block block in blocks.GetComponentsInChildren<Block>()) {
            block.Reset();
        }
        StartCoroutine(WaitToCall(() => {
            state = GameState.Playing;
        }, 1));
    }

    public void FinishLevel()
    {
        state = GameState.Finished;
        print(blocks);
        foreach(Block block in blocks.GetComponentsInChildren<Block>()) {
            block.Disappear();
        }
        StartCoroutine(WaitToCall(() => {
            // SceneManager.UnloadSceneAsync(currentLevel);
            TryGetNextLevel();
        }, 1));
    }

    private void TryGetNextLevel()
    {
        if (currentLevel < numLevels) {
            currentLevel++;
            levelCounter.text = "" + currentLevel;
            grid.OpenLevel(currentLevel);
            swipeCounter.ResetSwipes();
            // SceneManager.sceneLoaded += OnLevelFinishedLoading;
            // SceneManager.LoadScene(currentLevel, LoadSceneMode.Additive);
        }
    }

    public void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
        blocks = GameObject.Find("/Blocks").transform;
        grid = GameObject.Find("/GameGrid").GetComponent<GameGrid>();
        state = GameState.Playing;
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    private void StartPlaying() {
        Vector3 goalPosition = startingGrid.Find("goal block").position;
        Block color = startingGrid.Find("color block").gameObject.GetComponent<Block>();
        color.MoveTo(goalPosition);
        startingUI.GetComponent<Animator>().SetTrigger("fade-out");
        StartCoroutine(WaitToCall(() => {
            foreach(Block block in startingGrid.GetComponentsInChildren<Block>()) {
                block.Disappear();
            }
        }, .5f));
        StartCoroutine(WaitToCall(() => {
            startingUI.SetActive(false);
            startingGrid.gameObject.SetActive(false);
            Camera.main.orthographicSize = 10;
            playingUI.SetActive(true);
            grid.OpenLevel(currentLevel);
            // SceneManager.sceneLoaded += OnLevelFinishedLoading;
            // SceneManager.LoadScene(currentLevel, LoadSceneMode.Additive);
        }, 1));
    }

    IEnumerator WaitToCall(Func functionToCall, float timeToWait) {
        yield return new WaitForSeconds(timeToWait);
        functionToCall();
    }
}
