using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using TMPro;

public class WaveFunction : MonoBehaviour
{
    public int dimensions;
    public Tile startTile;
    public Tile[] tileObjects;
    public List<Cell> gridComponents;
    public Tile[] gridTile;
    public Cell cellObj;
    public int collapsedCells;
    public Transform world;
    private bool isCheckingEntropy;
    public TextMeshProUGUI restartText;

    void Awake()
    {
        // Set start tile and dimensions
        if (TransportData.transportTile != null) startTile = TransportData.transportTile;
        dimensions = TransportData.transportDimensions;
        Debug.Log(dimensions);
        
        // Initialising generation
        gridComponents = new List<Cell>();
        InitializeGrid();
    }

    void InitializeGrid()
    {
        // Fill gridComponents with empty cells
        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                float offset = (dimensions - 1) / 2f;
                Vector2 pos = new Vector2(x - offset, y - offset);

                Cell newCell = new Cell(pos);
                newCell.CreateCell(false, tileObjects);
                gridComponents.Add(newCell);
            }
        }

        // Force collapse start tile and water/land depending on start tile
        gridTile = new Tile[gridComponents.Count];
        CollapseCell(gridComponents, startTile, 0);
        if (startTile.isWalkable) CollapseCell(gridComponents, tileObjects[0], gridComponents.Count-1);
        else CollapseCell(gridComponents, tileObjects[tileObjects.Length-1], gridComponents.Count - 1);

        // Start WFC
        StartCoroutine(CheckEntropy());
    }

    IEnumerator CheckEntropy()
    {
        if (isCheckingEntropy) yield break; // Make sure only one WFC is happening
        isCheckingEntropy = true;

        // Extract all uncollapsed cells
        List<Cell> tempGrid = new List<Cell>(gridComponents);
        tempGrid.RemoveAll(c => c.collapsed);

        // Store number of collapsed cells
        collapsedCells = gridComponents.Count - tempGrid.Count;

        // Sort tempGrid based on entropy
        tempGrid.Sort((a, b) => { return a.tileOptions.Length - b.tileOptions.Length; });

        int arrLength = tempGrid[0].tileOptions.Length; // Lowest entropy cell
        int stopIndex = default;

        // Gets the index of the last cell with the same entropy as the lowest entropy cell
        for (int i = 1; i < tempGrid.Count; i++)
        {
            if (tempGrid[i].tileOptions.Length > arrLength)
            {
                stopIndex = i;
                break;
            }
        }

        // tempGrid only contains cells of the lowest entropy
        if (stopIndex > 0)
        {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }

        // Collapse all cells in tempGrid
        yield return new WaitForSeconds(0.01f);
        isCheckingEntropy = false;
        CollapseCell(tempGrid);
    }

    void CollapseCell(List<Cell> tempGrid, Tile forcedTile = null, int index = -1)
    {
        // Collapse a random cell in tempGrid
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);
        Cell cellToCollapse = tempGrid[randIndex];

        // If index is passed in, collapse at the passed in value
        if (index != -1) cellToCollapse = tempGrid[index];

        // If entropy is 0, there is an error so it restarts
        if (cellToCollapse.tileOptions.Length == 0)
        {
            Debug.LogWarning("Conflict detected, restarting");
            Restart();
            return;
        }

        // Collapses cell
        cellToCollapse.collapsed = true;

        Tile selectedTile;

        // If forcedTile is passed in, collapse the cell to the forced tile otherwise take a random option from tileOptions
        if (forcedTile != null) selectedTile = forcedTile;
        else selectedTile = RandomWeightedTile(cellToCollapse.tileOptions);

        Debug.Log(selectedTile);

        // Cell updated
        cellToCollapse.tileOptions = new Tile[] { selectedTile };

        // Instantiate tile
        Tile instantiatedTile = Instantiate(selectedTile, cellToCollapse.pos, Quaternion.identity, world);

        // Add instantated tile to gridTile
        int instantiatedIndex = gridComponents.IndexOf(cellToCollapse);
        gridTile[instantiatedIndex] = instantiatedTile;

        // Update grid
        UpdateGeneration();
    }

    Tile RandomWeightedTile(Tile[] tileOptions) // Higher weight means higher chance
    {
        int totalWeight = 0;
        for (int i = 0; i < tileOptions.Length; i++)
        {
            totalWeight += tileOptions[i].weight;
        }

        int rand = UnityEngine.Random.Range(0, totalWeight);
        
        for (int i = 0; i < tileOptions.Length-1; i++)
        {
            rand -= tileOptions[i].weight;
            if (rand <= 0) return tileOptions[i];
        }
        return tileOptions[tileOptions.Length - 1];
    }

    void UpdateGeneration()
    {
        // Create list of size of gridComponents
        List<Cell> newGenerationCell = new List<Cell>(gridComponents);

        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                var index = x + y * dimensions;
                // If cell is collapsed, add it to newGenerationCell, otherwise we update its entropy
                if (gridComponents[index].collapsed)
                {
                    newGenerationCell[index] = gridComponents[index];
                }
                else
                {
                    // Assumes cell has every available tile option
                    List<Tile> options = new List<Tile>();
                    foreach (Tile t in tileObjects)
                    {
                        options.Add(t);
                    }

                    // Filter out tile options

                    // Update above
                    if (y > 0)
                    {
                        Cell up = gridComponents[x + (y - 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in up.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].upNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    // Update right
                    if (x < dimensions - 1)
                    {
                        Cell right = gridComponents[x + 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in right.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].leftNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    // Update down
                    if (y < dimensions - 1)
                    {
                        Cell down = gridComponents[x + (y + 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in down.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].downNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    // Update left
                    if (x > 0)
                    {
                        Cell left = gridComponents[x - 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in left.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].rightNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    // Regenerate the cell with its new entropy and add it to newGenerationCell
                    Tile[] newTileList = new Tile[options.Count];

                    for (int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[index].RecreateCell(newTileList);
                }
            }
        }

        // Updates gridComponents
        gridComponents = newGenerationCell;

        // Continue if still has cells to collapse, otherwise start simulation
        if (gridComponents.Any(c => !c.collapsed))
        {
            StartCoroutine(CheckEntropy());
        }
        else
        {
            Debug.Log("Complete");
            SimulationManager.Instance.StartTick();
        }

    }

    void CheckValidity(List<Tile> optionList, List<Tile> validOption) // Update optionList by removing all elements not in validOption
    {
        for (int x = optionList.Count - 1; x >= 0; x--)
        {
            var element = optionList[x];
            if (!validOption.Contains(element))
            {
                optionList.RemoveAt(x);
            }
        }
    }

    void Restart()
    {
        // Restart text
        StartCoroutine(ShowRestartText());

        // Clear all children
        foreach (Transform tile in world)
        {
            Destroy(tile.gameObject);
        }

        // Reset state
        gridComponents.Clear();
        gridTile = null;
        collapsedCells = 0;
        isCheckingEntropy = false;

        // Regenerate
        InitializeGrid();
    }

    IEnumerator ShowRestartText()
    {
        restartText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        restartText.gameObject.SetActive(false);
    }
}