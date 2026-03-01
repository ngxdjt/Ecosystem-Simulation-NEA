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
        if (TransportData.transportTile != null) startTile = TransportData.transportTile;
        dimensions = TransportData.transportDimensions;
        Debug.Log(dimensions);
        gridComponents = new List<Cell>();
        InitializeGrid();
    }

    void InitializeGrid()
    {
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

        gridTile = new Tile[gridComponents.Count];
        CollapseCell(gridComponents, startTile, 0);
        if (startTile.isWalkable) CollapseCell(gridComponents, tileObjects[0], gridComponents.Count-1);
        else CollapseCell(gridComponents, tileObjects[tileObjects.Length-1], gridComponents.Count - 1);
        StartCoroutine(CheckEntropy());
    }

    IEnumerator CheckEntropy()
    {
        if (isCheckingEntropy) yield break;
        isCheckingEntropy = true;

        List<Cell> tempGrid = new List<Cell>(gridComponents);

        tempGrid.RemoveAll(c => c.collapsed);

        collapsedCells = gridComponents.Count - tempGrid.Count;

        tempGrid.Sort((a, b) => { return a.tileOptions.Length - b.tileOptions.Length; });

        int arrLength = tempGrid[0].tileOptions.Length;
        int stopIndex = default;

        for (int i = 1; i < tempGrid.Count; i++)
        {
            if (tempGrid[i].tileOptions.Length > arrLength)
            {
                stopIndex = i;
                break;
            }
        }

        if (stopIndex > 0)
        {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }

        yield return new WaitForSeconds(0.01f);
        isCheckingEntropy = false;
        CollapseCell(tempGrid);
    }

    void CollapseCell(List<Cell> tempGrid, Tile forcedTile = null, int index = -1)
    {
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);
        Cell cellToCollapse = tempGrid[randIndex];

        if (index != -1) cellToCollapse = tempGrid[index];

        if (cellToCollapse.tileOptions.Length == 0)
        {
            Debug.LogWarning("Conflict detected, restarting");
            Restart();
            return;
        }

        cellToCollapse.collapsed = true;

        Tile selectedTile;

        if (forcedTile != null) selectedTile = forcedTile;
        else selectedTile = RandomWeightedTile(cellToCollapse.tileOptions);

        Debug.Log(selectedTile);

        cellToCollapse.tileOptions = new Tile[] { selectedTile };

        Tile instantiatedTile = Instantiate(selectedTile, cellToCollapse.pos, Quaternion.identity, world);

        int instantiatedIndex = gridComponents.IndexOf(cellToCollapse);
        gridTile[instantiatedIndex] = instantiatedTile;

        UpdateGeneration();
    }

    Tile RandomWeightedTile(Tile[] tileOptions)
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
        List<Cell> newGenerationCell = new List<Cell>(gridComponents);

        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                var index = x + y * dimensions;
                if (gridComponents[index].collapsed)
                {
                    newGenerationCell[index] = gridComponents[index];
                }
                else
                {
                    List<Tile> options = new List<Tile>();
                    foreach (Tile t in tileObjects)
                    {
                        options.Add(t);
                    }

                    // update above
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

                    // update right
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

                    // update down
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

                    // update left
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

                    Tile[] newTileList = new Tile[options.Count];

                    for (int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[index].RecreateCell(newTileList);
                }
            }
        }

        gridComponents = newGenerationCell;

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

    void CheckValidity(List<Tile> optionList, List<Tile> validOption)
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