using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDebug : MonoBehaviour
{

    private static SpriteDebug instance;
    private SpriteCreator sprComponent;
    private WaveFunctionGrid2D grid;



    private void Start()
    {
        instance = this;
        sprComponent = GetComponent<SpriteCreator>();
        grid = sprComponent.grid;
    }

    public static SpriteDebug Instance() 
    { 
     
        return instance;
    }

    public void setColors(Color[] col)
    {
        if (grid == null)
        {
            sprComponent = GetComponent<SpriteCreator>();
            grid = sprComponent.grid;
        }
        grid.setColors(col);
    }

    public void drawWaveTile(WaveTile2D wt)
    {
        for (int x = 0; x < wt.size; x++) {
            for (int y = 0; y < wt.size; y++)
            {
                grid.SetPixelAt(x, y, wt.tileContent[x,y]);
            }
        }
        sprComponent.ApplyChanges();
        
    }
}
