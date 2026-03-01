using UnityEngine;
using System.Collections;

public class Wolf : Animal
{
    protected override Vector2Int SearchFood()
    {
        Vector2Int pointer = pos;
        int passes;
        Vector2Int[] sightTriangle = CalculateSightTriangle();

        switch (facing)
        {
            case Direction.Up:
                passes = sightTriangle[1].y - sightTriangle[0].y + 1;
                while (passes > 0)
                {
                    Vector2Int verticalReflectedPointer = new Vector2Int(2 * pos.x - pointer.x, pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pointer)].GetComponentInChildren<Carcass>() != null || 
                            IsAdjacentToSheep(pointer)) 
                            return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, verticalReflectedPointer) && 
                           (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, verticalReflectedPointer)].GetComponentInChildren<Carcass>() != null || 
                            IsAdjacentToSheep(verticalReflectedPointer)))
                            return verticalReflectedPointer;
                        pointer.x++;
                    }
                    else
                    {
                        pointer.x = pos.x;
                        pointer.y++;
                        passes--;
                    }
                }
                break;

            case Direction.Right:
                passes = sightTriangle[1].x - sightTriangle[0].x + 1;
                while (passes > 0)
                {
                    Vector2Int horizontalReflectedPointer = new Vector2Int(pointer.x, 2 * pos.y - pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pointer)].GetComponentInChildren<Carcass>() != null ||
                            IsAdjacentToSheep(pointer))
                            return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, horizontalReflectedPointer) &&
                           (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, horizontalReflectedPointer)].GetComponentInChildren<Carcass>() != null ||
                            IsAdjacentToSheep(horizontalReflectedPointer)))
                            return horizontalReflectedPointer;
                        pointer.y++;
                    }
                    else
                    {
                        pointer.y = pos.y;
                        pointer.x++;
                        passes--;
                    }
                }
                break;

            case Direction.Down:
                passes = sightTriangle[0].y - sightTriangle[1].y + 1;
                while (passes > 0)
                {
                    Vector2Int verticalReflectedPointer = new Vector2Int(2 * pos.x - pointer.x, pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pointer)].GetComponentInChildren<Carcass>() != null ||
                            IsAdjacentToSheep(pointer))
                            return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, verticalReflectedPointer) &&
                           (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, verticalReflectedPointer)].GetComponentInChildren<Carcass>() != null ||
                            IsAdjacentToSheep(verticalReflectedPointer)))
                            return verticalReflectedPointer;
                        pointer.x++;
                    }
                    else
                    {
                        pointer.x = pos.x;
                        pointer.y--;
                        passes--;
                    }
                }
                break;

            case Direction.Left:
                passes = sightTriangle[0].x - sightTriangle[1].x + 1;
                while (passes > 0)
                {
                    Vector2Int horizontalReflectedPointer = new Vector2Int(pointer.x, 2 * pos.y - pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pointer)].GetComponentInChildren<Carcass>() != null ||
                            IsAdjacentToSheep(pointer))
                            return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, horizontalReflectedPointer) &&
                           (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, horizontalReflectedPointer)].GetComponentInChildren<Carcass>() != null ||
                            IsAdjacentToSheep(horizontalReflectedPointer)))
                            return horizontalReflectedPointer;
                        pointer.y++;
                    }
                    else
                    {
                        pointer.y = pos.y;
                        pointer.x--;
                        passes--;
                    }
                }
                break;
        }

        if (currentTarget != Vector2Int.zero) return currentTarget;
        Vector2Int randomTile;
        int attempts = 0;
        do
        {
            randomTile = new Vector2Int(UnityEngine.Random.Range(0, map.dimensions), UnityEngine.Random.Range(0, map.dimensions));
            attempts++;
            if (attempts == 100) return pos;
        }
        while (!map.gridTile[Geometry.CoordinateToIndex(map.dimensions, randomTile)].isWalkable && map.gridTile[Geometry.CoordinateToIndex(map.dimensions, randomTile)].occupant != null);
        return randomTile;
    }

    protected override IEnumerator Consume()
    {
        int tileIndex = Geometry.CoordinateToIndex(map.dimensions, pos);
        Carcass carcass = map.gridTile[tileIndex].GetComponentInChildren<Carcass>();

        if (carcass != null)
        {
            animalSprite.sprite = sprites[(int) SpriteState.Consuming];

            StartCoroutine(carcass.Eaten());
            hunger = Mathf.Max(0f, hunger - 0.4f);

            yield return new WaitForSeconds(1f / simulationManager.tickRate);

            animalSprite.sprite = sprites[(int)SpriteState.Standing];
            yield break;
        }

        if (IsAdjacentToSheep(pos))
        {
            KillAdjacentSheep();
            yield break;
        }
    }

    protected override bool CanEat()
    {
        if (IsAdjacentToSheep(pos)) return true;

        int tileIndex = Geometry.CoordinateToIndex(map.dimensions, pos);
        Carcass carcass = map.gridTile[tileIndex].GetComponentInChildren<Carcass>();

        return (carcass != null);
    }

    private bool IsAdjacentToSheep(Vector2Int pos)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbourPos = pos + direction;
            if (!Geometry.IsInsideGrid(map.dimensions, neighbourPos)) continue; // Boundary check
            if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, neighbourPos)].GetComponentInChildren<Carcass>()) continue; // Prevent infinite killing
            if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, neighbourPos)].occupant is Sheep) return true;
        }
        return false;
    }

    private void KillAdjacentSheep()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbourPos = pos + direction;
            if (!Geometry.IsInsideGrid(map.dimensions, neighbourPos)) continue; // Boundary check

            Animal occupant = map.gridTile[Geometry.CoordinateToIndex(map.dimensions, neighbourPos)].occupant;
            if (occupant is Sheep && occupant != null)
            {
                occupant.Die();
                hunger = Mathf.Max(0f, hunger - 0.2f);
            }
        }
    }
}
