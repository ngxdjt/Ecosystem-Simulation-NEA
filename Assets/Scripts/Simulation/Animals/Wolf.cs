using UnityEngine;
using System.Collections;

public class Wolf : Animal
{
    protected override Vector2Int SearchFood()
    {
        Vector2Int pointer = pos; // Initiate pointer to current position
        int passes;
        Vector2Int[] sightTriangle = CalculateSightTriangle();

        switch (facing)
        {
            case Direction.Up:
                passes = sightTriangle[1].y - sightTriangle[0].y + 1; // Only pass for as many layers the triangle has
                while (passes > 0)
                {
                    Vector2Int verticalReflectedPointer = new Vector2Int(2 * pos.x - pointer.x, pointer.y); // Reflect the pointer across the centre of the triangle
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer)) // Boundary check
                    {
                        // Return position of target if found else increment pointer
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
                        // Move to next layer
                        pointer.x = pos.x;
                        pointer.y++;
                        passes--;
                    }
                }
                break;

            case Direction.Right:
                passes = sightTriangle[1].x - sightTriangle[0].x + 1; // Only pass for as many layers the triangle has
                while (passes > 0)
                {
                    Vector2Int horizontalReflectedPointer = new Vector2Int(pointer.x, 2 * pos.y - pointer.y); // Reflect the pointer across the centre of the triangle
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer)) // Boundary check
                    {
                        // Return position of target if found else increment pointer
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
                        // Move to next layer
                        pointer.y = pos.y;
                        pointer.x++;
                        passes--;
                    }
                }
                break;

            case Direction.Down:
                passes = sightTriangle[0].y - sightTriangle[1].y + 1; // Only pass for as many layers the triangle has
                while (passes > 0)
                {
                    Vector2Int verticalReflectedPointer = new Vector2Int(2 * pos.x - pointer.x, pointer.y); // Reflect the pointer across the centre of the triangle
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer)) // Boundary check
                    {
                        // Return position of target if found else increment pointer
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
                        // Move to next layer
                        pointer.x = pos.x;
                        pointer.y--;
                        passes--;
                    }
                }
                break;

            case Direction.Left:
                passes = sightTriangle[0].x - sightTriangle[1].x + 1; // Only pass for as many layers the triangle has
                while (passes > 0)
                {
                    Vector2Int horizontalReflectedPointer = new Vector2Int(pointer.x, 2 * pos.y - pointer.y); // Reflect the pointer across the centre of the triangle
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer)) // Boundary check
                    {
                        // Return position of target if found else increment pointer
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
                        // Move to next layer
                        pointer.y = pos.y;
                        pointer.x--;
                        passes--;
                    }
                }
                break;
        }

        // If animal has a target already, return that
        if (currentTarget != Vector2Int.zero) return currentTarget;
        Vector2Int randomTile;
        int attempts = 0;

        // Get a random valid tile and if cannot be found after 100 attempts give up
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
        // Get carcass of current position
        int tileIndex = Geometry.CoordinateToIndex(map.dimensions, pos);
        Carcass carcass = map.gridTile[tileIndex].GetComponentInChildren<Carcass>();

        if (carcass != null)
        {
            // Change the animal sprite
            animalSprite.sprite = sprites[(int) SpriteState.Consuming];

            // Update carcass
            StartCoroutine(carcass.Eaten());

            // Wait 1 tick
            yield return new WaitForSeconds(1f / simulationManager.tickRate);

            // Update animal
            hunger = Mathf.Max(0f, hunger - 0.4f);
            animalSprite.sprite = sprites[(int)SpriteState.Standing];
            yield break;
        }

        // Kill all adjacent sheep if next to sheep and not on carcass
        if (IsAdjacentToSheep(pos))
        {
            KillAdjacentSheep();
            yield break;
        }
    }

    protected override bool CanEat()
    {
        // If adjacent to sheep it can eat
        if (IsAdjacentToSheep(pos)) return true;

        // Get carcass at current position
        int tileIndex = Geometry.CoordinateToIndex(map.dimensions, pos);
        Carcass carcass = map.gridTile[tileIndex].GetComponentInChildren<Carcass>();

        // Check if a carcass exists at position
        return (carcass != null);
    }

    private bool IsAdjacentToSheep(Vector2Int pos)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        // Iterate through cardinal directions
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbourPos = pos + direction;
            if (!Geometry.IsInsideGrid(map.dimensions, neighbourPos)) continue; // Boundary check
            if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, neighbourPos)].GetComponentInChildren<Carcass>()) continue; // Prevent infinite killing
            if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, neighbourPos)].occupant is Sheep) return true; // If sheep found on adjacent, return true
        }
        return false;
    }

    private void KillAdjacentSheep()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        // Iterate through cardinal directions
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbourPos = pos + direction;
            if (!Geometry.IsInsideGrid(map.dimensions, neighbourPos)) continue; // Boundary check

            Animal occupant = map.gridTile[Geometry.CoordinateToIndex(map.dimensions, neighbourPos)].occupant;
            if (occupant is Sheep && occupant != null) // Check if occupant exists and is a sheep
            {
                // Kill sheep and decrease hunger
                occupant.Die();
                hunger = Mathf.Max(0f, hunger - 0.2f);
            }
        }
    }
}
