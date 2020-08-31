using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public BlockType type;
    public Color color;
    public bool inGrid = true;
    public float speed = 35;

    private Animator anim;
    private GameGrid grid;
    private Vector3 targetPosition;
    private Vector3 originPosition;
    private bool moving = false;

    private void Start() {
        // Set animator
        anim = GetComponent<Animator>();

        // Set color
        GetComponent<SpriteRenderer>().color = color;

        // Set in grid
        if (inGrid) {
            grid = GameObject.Find("/GameGrid").GetComponent<GameGrid>();
            if (type != BlockType.Cell) SetInGrid();
        }
    }

    public void MoveTo(Vector3 position)
    {
        targetPosition = new Vector3(
            position.x,
            position.y,
            transform.position.z
        );
        if (!moving) {
            moving = true;
            StartCoroutine(Moving());
        }
    }

    IEnumerator Moving()
    {
        while (Vector3.Distance(targetPosition, transform.position) > .01f) {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        transform.position = targetPosition;
        moving = false;
    }

    public void ReachAndReturn(int dirX, int dirY) {
        Vector3 reachPosition = new Vector3(
            transform.position.x + .25f * dirX,
            transform.position.y + .25f * dirY,
            transform.position.z
        );
        if (!moving) {
            moving = true;
            StartCoroutine(ReachingAndReturning(reachPosition, transform.position));
        }
    }

    IEnumerator ReachingAndReturning(Vector3 reachPosition, Vector3 returnPosition) {
        // Reaching
        while (Vector3.Distance(reachPosition, transform.position) > .01f) {
            transform.position = Vector3.MoveTowards(transform.position, reachPosition, speed/2 * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        transform.position = reachPosition;
        // Returning
        while (Vector3.Distance(returnPosition, transform.position) > .01f) {
            transform.position = Vector3.MoveTowards(transform.position, returnPosition, speed/2 * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        transform.position = returnPosition;
        moving = false;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        switch(type) {
            case BlockType.Color:
                if (other.gameObject.GetComponent<Block>().type == BlockType.Goal) {
                    if (inGrid) grid.ReachedGoal();
                }
                break;
            case BlockType.Cell:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("idle")) anim.SetTrigger("pulse");
                break;
            case BlockType.Obstacle:
                if (other.gameObject.GetComponent<Block>().type == BlockType.Color) {
                    GameObject.Find("GameStateManager").GetComponent<GameStateManager>().ResetLevel();
                }
                break;
        }
    }

    public void Reset() {
        // Reset all triggers
        anim.SetTrigger("reset");
    }

    public void ClearAnimatorParameters() {
        foreach(AnimatorControllerParameter parameter in anim.parameters) {
            if (parameter.type == AnimatorControllerParameterType.Trigger) {
                print(parameter.name);
                anim.ResetTrigger(parameter.name);
            }
        }
    }

    public void MoveToOrigin() {
        transform.position = originPosition;
        if (type != BlockType.Cell) SetInGrid();
    }

    public void SetInGrid() {
        (int row, int col) = grid.GetGridPosition(transform.position);
        grid.PlaceBlock(row, col, this);
    }

    public void Disappear() {
        anim.SetTrigger("disappear");
    }

    public void Delete() {
        Destroy(gameObject);
    }

    public void SetColor(Color newColor) {
        color = newColor;
        GetComponent<SpriteRenderer>().color = newColor;
    }

    public void SetOrigin(Vector3 newPosition) {
        originPosition = newPosition;
    }
}
