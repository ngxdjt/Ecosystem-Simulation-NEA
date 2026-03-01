using UnityEngine;
using System.Collections;

public class Sheep : Animal
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
                        if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pointer)].GetComponent<PlantGrowth>().growthLevel == 3) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, verticalReflectedPointer) && map.gridTile[Geometry.CoordinateToIndex(map.dimensions, verticalReflectedPointer)].GetComponent<PlantGrowth>().growthLevel == 3) return verticalReflectedPointer;
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
                        if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pointer)].GetComponent<PlantGrowth>().growthLevel == 3) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, horizontalReflectedPointer) && map.gridTile[Geometry.CoordinateToIndex(map.dimensions, horizontalReflectedPointer)].GetComponent<PlantGrowth>().growthLevel == 3) return horizontalReflectedPointer;
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
                        if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pointer)].GetComponent<PlantGrowth>().growthLevel == 3) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, verticalReflectedPointer) && map.gridTile[Geometry.CoordinateToIndex(map.dimensions, verticalReflectedPointer)].GetComponent<PlantGrowth>().growthLevel == 3) return verticalReflectedPointer;
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
                        if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pointer)].GetComponent<PlantGrowth>().growthLevel == 3) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, horizontalReflectedPointer) && map.gridTile[Geometry.CoordinateToIndex(map.dimensions, horizontalReflectedPointer)].GetComponent<PlantGrowth>().growthLevel == 3) return horizontalReflectedPointer;
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
        animalSprite.sprite = sprites[(int) SpriteState.Consuming];

        yield return new WaitForSeconds(1f / simulationManager.tickRate);

        int tileIndex = Geometry.CoordinateToIndex(map.dimensions, pos);
        PlantGrowth plant = map.gridTile[tileIndex].GetComponent<PlantGrowth>();

        plant.Eaten();

        hunger = Mathf.Max(0f, hunger - 0.4f);
        animalSprite.sprite = sprites[(int) SpriteState.Standing];
    }
    
    protected override bool CanEat()
    {
        int tileIndex = Geometry.CoordinateToIndex(map.dimensions, pos);
        PlantGrowth plant = map.gridTile[tileIndex].GetComponent<PlantGrowth>();

        if (plant.growthLevel == 3) return true;

        return false;
    }
}