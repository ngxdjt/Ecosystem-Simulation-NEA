using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public bool collapsed;
    public Tile[] tileOptions;
    public Vector2 pos;

    public Cell(Vector2 pos)
    {
        this.pos = pos;
    } 

    public void CreateCell(bool collapseState, Tile[] tiles)
    {
        collapsed = collapseState;
        tileOptions = tiles;
    }

    public void RecreateCell(Tile[] tiles)
    {
        tileOptions = tiles;
    }
}
