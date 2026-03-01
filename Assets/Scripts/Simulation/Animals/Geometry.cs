using UnityEngine;

public static class Geometry
{
    public static bool IsInsideTriangle(Vector2Int[] triangle, Vector2Int point)
    {
        float area = CalculateArea(triangle);
        float area1 = CalculateArea( new Vector2Int[] { point, triangle[1], triangle[2] } );
        float area2 = CalculateArea( new Vector2Int[] { triangle[0], point, triangle[2] } );
        float area3 = CalculateArea( new Vector2Int[] { triangle[0], triangle[1], point } );

        return Mathf.Abs(area - (area1 + area2 + area3)) < 0.0001f;
    }

    public static float CalculateArea(Vector2Int[] triangle)
    {
        Vector2Int vertex1 = triangle[0];
        Vector2Int vertex2 = triangle[1];
        Vector2Int vertex3 = triangle[2];
        return Mathf.Abs((vertex1.x * (vertex2.y - vertex3.y) + vertex2.x * (vertex3.y - vertex1.y) + vertex3.x * (vertex1.y - vertex2.y)) / 2f);
    }

    public static bool IsInsideGrid(int dimensions, Vector2Int point)
    {
        return point.x >= 0 && point.y >= 0 && point.x < dimensions && point.y < dimensions;
    }

    public static int CoordinateToIndex(int dimensions, Vector2Int coordinate)
    {
        return coordinate.x + coordinate.y * dimensions;
    }

    public static Vector2Int IndexToCoordinate(int dimensions, int index)
    {
        return new Vector2Int(index % dimensions, index / dimensions);
    }
}
