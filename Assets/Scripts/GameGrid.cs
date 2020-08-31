using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum BlockType {
    Color,
    Obstacle,
    Wall,
    Goal,
    Cell
}

[System.Serializable]
public class LevelBlocks{
    public List<Vector3> colorBlocks;
    public List<Vector3> goalBlocks;
    public List<Vector3> wallBlocks; 
}

[System.Serializable]
public class Level {
    public string numGoals;
    public int numCols;
    public int numRows;


}

public class GameGrid : MonoBehaviour
{
    public GameStateManager gameState;
    public GameObject colorBlock;
    public GameObject goalBlock;
    public GameObject obstacleBlock;
    public GameObject wallBlock;
    public GameObject cellBlock;
    public Transform blocksHolder;
    public Transform cellBlocksHolder;
    public SwipeCounter swipeCounter;

    private int cols;
    private int rows;
    private float cellSize;
    private Block[][] grid;
    private string[] solution;
    private int numGoals;
    private int completedGoals = 0;
    private float moveCooldown = 0;

    private HashSet<BlockType> MOVEABLES = new HashSet<BlockType>{
        BlockType.Color,
        BlockType.Obstacle
    };

    public void OpenLevel(int levelNum) {
        // Reset game values
        cols = 0;
        rows = 0;
        cellSize = 0;
        numGoals = 0;
        completedGoals = 0;

        // InitGrid shoudl be called with a level number 
        // it then reads the appropriate text file and parses it into a level
        string path = Application.dataPath + "/levels/Level " + levelNum + ".txt";
        StreamReader file = new StreamReader(path);
        List<string> lines = new List<string>();
        string line;
        while((line = file.ReadLine()) != null)  
        {  
            lines.Add(line);
        }  
        file.Close();

        string[] gridSize = lines[0].Split(',');
        cols = int.Parse(gridSize[0]);
        rows = int.Parse(gridSize[1]);
        cellSize = float.Parse(gridSize[2]);

        // Setup cellblocks in grid
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                Block block = Instantiate(cellBlock).GetComponent<Block>();
                block.transform.parent = cellBlocksHolder;
                block.transform.localPosition = GetCellPosition(i,j);
                block.SetOrigin(block.transform.position);
            }
        }

        // Setup grid Block matrix [rows by columns]
        grid = new Block[rows][];
        int currentLine = 1;
        // We iterate through the rows from top to bottom because that's how the levels
        // are written text (top rows to bottom rows)
        for (int i = rows - 1; i >= 0; i--) {
            grid[i] = new Block[cols];
            string[] blocks = lines[currentLine].Split(' ');
            for (int j = 0; j < cols; j++) {
                Block block = null;
                Color color = new Color(0, 0, 0);
                switch(blocks[j][0]) {
                    case 'c':
                        // A color block (color is defined by second character)
                        if (blocks[j][1] == 'r') color = new Color32(241, 62, 100, 255);
                        if (blocks[j][1] == 'b') color = new Color32(40, 102, 197, 255);
                        block = Instantiate(colorBlock).GetComponent<Block>();
                        block.SetColor(color);
                        break;
                    case 'g':
                        // A goal block (color is defined by second character)
                        if (blocks[j][1] == 'r') color = new Color32(241, 62, 100, 255);
                        if (blocks[j][1] == 'b') color = new Color32(40, 102, 197, 255);
                        block = Instantiate(goalBlock).GetComponent<Block>();
                        block.SetColor(color);
                        numGoals++;
                        break;
                    case 'o':
                        // A wall block
                        block = Instantiate(obstacleBlock).GetComponent<Block>();
                        break;
                    case 'w':
                        // A wall block
                        block = Instantiate(wallBlock).GetComponent<Block>();
                        break;
                }
                if (block != null) {
                    block.transform.position = GetCellPosition(i,j);
                    block.SetOrigin(block.transform.position);
                    block.transform.parent = blocksHolder;
                }
            }
            currentLine++;
        }

        // Get the level solution
        solution = lines[lines.Count - 1].Split(',');
        swipeCounter.SetTarget(solution.Length);

        // Set GameState to playing
        GameStateManager.state = GameState.Playing;
    }

    private void InitGrid() {

        // Setup horizontal edges
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                Transform block = Instantiate(cellBlock).transform;
                block.parent = cellBlocksHolder;
                block.localPosition = GetCellPosition(i,j);
            }
        }
        // Setup vertical edges

        // Setup grid Block matrix [rows by columns]
        grid = new Block[rows][];
        for (int i = 0; i < rows; i++) {
            grid[i] = new Block[cols];
        }
    }

    public void Reset() {
        completedGoals = 0;
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                grid[i][j] = null;
            }
        }
    }

    public void ReachedGoal() {
        completedGoals++;
        if (completedGoals == numGoals) gameState.FinishLevel();
    }

    public void PlaceBlock(int col, int row, Block block) {
        grid[row][col] = block;
    }

    private void Update() {
        if (GameStateManager.state != GameState.Playing) return;

        if (SwipeHandler.swiped && Time.time >= moveCooldown) {
            SwipeMove();
            moveCooldown = Time.time + .5f;
        }
    }

    private void SwipeMove() {
        // Get direction of swipe and try to move all movable blocks in that direction
        swipeCounter.AddSwipe();
        Vector3 swipe = SwipeHandler.touchUp - SwipeHandler.touchDown;
        if (Mathf.Abs(swipe.x) >= Mathf.Abs(swipe.y)) {
            if (swipe.x >= 0) MoveBlocksRight();
            else MoveBlocksLeft();
        } else {
            if (swipe.y >= 0) MoveBlocksUp();
            else MoveBlocksDown();
        }
    }

    public (int, int) GetGridPosition(Vector3 position) {
        int x = (int) ((position.x)/cellSize + cols/2);
        int y = (int) ((position.y)/cellSize + rows/2);
        return (x, y);
    }

    private Vector3 GetCellPosition(int row, int col) {
        return new Vector3(
            (col - cols/2)*cellSize,
            (row - rows/2)*cellSize,
            0
        );
    }

    private bool IsCorrectGoal(Block movingBlock, Block potentialGoal) {
        if (potentialGoal.type != BlockType.Goal) return false;
        return movingBlock.color == potentialGoal.color;
    }

    private bool IsObstacleHit(Block potentialObstacle, Block potentialColor) {
        if (potentialColor.type != BlockType.Color) return false;
        if (potentialObstacle.type != BlockType.Obstacle) return false;
        return true;
    }

    // Returns true if the block should stop
    private bool HandleBlockCollisions(int rowA, int colA, int rowB, int colB) {
        if (grid[rowA][colA] == null) return true;
        if (!MOVEABLES.Contains(grid[rowA][colA].type)) return true;
        bool hitGoal = false;
        bool hitObstacle = false;
        if (grid[rowB][colB] != null) {
            hitGoal = IsCorrectGoal(grid[rowA][colA], grid[rowB][colB]);
            hitObstacle = IsObstacleHit(grid[rowA][colA], grid[rowB][colB]);
            if (!hitGoal && !hitObstacle) return true;
        }
        if (!hitGoal) grid[rowB][colB] = grid[rowA][colA];
        grid[rowA][colA].MoveTo(GetCellPosition(rowB, colB));
        grid[rowA][colA] = null;
        return false;
    }

    private void MoveBlocksUp() {
        // Shake blocks that can't move
        for (int i = 0; i < cols; i++) {
            if (grid[rows-1][i] == null || !MOVEABLES.Contains(grid[rows-1][i].type)) continue;
            grid[rows-1][i].ReachAndReturn(0, 1);
        }
        print("moving blocks up");
        for (int i = rows-2; i >= 0; i--) {
            for (int j = 0; j < cols; j++) {
                // Keep trying to move block up until it can't
                int row = i;
                while (row < rows - 1) {
                    // Check if block is movable
                    if (HandleBlockCollisions(row, j, row+1, j)) break;
                    row++;
                }
            }
        }
    }

    private void MoveBlocksDown() {
        for (int i = 0; i < cols; i++) {
            if (grid[0][i] == null || !MOVEABLES.Contains(grid[0][i].type)) continue;
            grid[0][i].ReachAndReturn(0, -1);
        }
        print("moving blocks down");
        for (int i = 1; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                int row = i;
                while (row > 0) {
                    // Check if block is movable
                    if (HandleBlockCollisions(row, j, row-1, j)) break;
                    row--;
                }
            }
        }
    }

    private void MoveBlocksRight() {
        // Shake blocks that can't move
        for (int i = 0; i < rows; i++) {
            if (grid[i][cols-1] == null || !MOVEABLES.Contains(grid[i][cols-1].type)) continue;
            grid[i][cols-1].ReachAndReturn(1, 0);
        }
        print("moving blocks right");
        for (int i = 0; i < rows; i++) {
            for (int j = cols - 2; j >= 0; j--) {
                int col = j;
                while (col < cols - 1) {
                    // Check if block is movable
                    if (HandleBlockCollisions(i, col, i, col+1)) break;
                    col++;
                }
            }
        }
    }

    private void MoveBlocksLeft() {
        // Shake blocks that can't move
        for (int i = 0; i < rows; i++) {
            if (grid[i][0] == null || !MOVEABLES.Contains(grid[i][0].type)) continue;
            grid[i][0].ReachAndReturn(-1, 0);
        }
        print("moving blocks left");
        for (int i = 0; i < rows; i++) {
            for (int j = 1; j < cols; j++) {
                int col = j;
                while (col > 0) {
                    // Check if block is movable
                    if (HandleBlockCollisions(i, col, i, col-1)) break;
                    col--;
                }
            }
        }
    }
}
