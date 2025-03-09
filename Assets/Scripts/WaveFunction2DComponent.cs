using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;



public class WaveFunctionGrid2D
{
    public const int TILESIZE = 3;

    public bool isCreatedProperly = false;
    public bool hasBeenModified = true;
    public int[,] gridContent;
    private Texture2D texture;
    private Sprite sprite;
    public Vector2Int size;
    public Color[] colors;
    public int maxID = 10;



    public WaveFunctionGrid2D( Vector2Int newSize, Color[] newColors)
    {
        colors = newColors;
        size = newSize;
        init();
    }

    private void refreshColor()
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                SetPixelAt(x, y, gridContent[x,y]);
            }
        }
    }

    public void setColors(Color[] newColors)
    {
        colors=newColors;
        refreshColor();
    }

    public void init()
    {
        texture = new Texture2D(size.x, size.y);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        gridContent = new int[size.x, size.y];
        Fill(0);
        updateSprite();
    }

    public Sprite GetSprite()
    {
        return sprite;
    }

    public void SetPixelAt(int x, int y, int newID)
    {
        hasBeenModified = true;
        newID = Mathf.Clamp(newID, 0, colors.Length - 1);
        x = Mathf.Clamp(x, 0, size.x - 1);
        y = Mathf.Clamp(y, 0, size.y - 1);
        gridContent[x,y] = newID;
        Color color = colors[newID];
        texture.SetPixel(x, y, color);
    }

    public int GetPixelAt(int x, int y)
    {
        return gridContent[x,y];
    }

    public void Fill(int id)
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                SetPixelAt(x, y, id);
            }
        }
    }
    public bool updateSprite()
    {
        if (hasBeenModified)
        {
            hasBeenModified = false;
            if (texture == null)
            {
                init();
            }
            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return true;
        }
        return false;
    }

    public int[,] getContent()
    {
        return gridContent;
    }

    public Vector2 getSize()
    {
        return size;
    }

    public int GetTileAt(Vector2Int pos)
    {
        if (pos.x <  0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y ) return -1;
        return gridContent[pos.x, pos.y];
    }

    public void launchWaveFunction(WaveFunctionGrid2D input)
    {
        Dictionary<int, List<WaveTile2D>> tiles = new Dictionary<int, List<WaveTile2D>>();
        //Create List for the wave tile
        for (int i = 0; i < input.maxID; i++)
        {
            tiles[i] = new List<WaveTile2D>();
        }
        Vector2 inputSize = input.getSize();
        for (int x = 0; x < input.gridContent.GetLength(0); x++)
        {
            for (int y = 0; y < input.gridContent.GetLength(1); y++)
            {
                int[,] tileContent = new int[TILESIZE, TILESIZE];
                int min = -Mathf.FloorToInt(TILESIZE/2);
                int max = Mathf.FloorToInt(TILESIZE / 2);

                for (int i = min ; i <= max; i++)
                {
                    for (int j = min; j <= max; j++)
                    {
                        tileContent[i -min, j - min] = input.GetTileAt(new Vector2Int(x + i, y + j));
                    }
                }
                WaveTile2D currentTile = new WaveTile2D(tileContent);
                bool existInList = false;
                //Check if it exist already in the list
                foreach (WaveTile2D tile in tiles[currentTile.getCenter()])
                {
                    if (tile == currentTile)
                    {
                        existInList = true;
                        break;
                    }
                }
                if (!existInList)
                {
                    tiles[currentTile.getCenter()].Add(currentTile);
                }
            }
        }
        Debug.Log("Test");
    }
}

public class WaveTile2D
{
    public int size;
    public int[,] tileContent;
    
    // -1 = Image Border

    public WaveTile2D(int[,] content)
    {
        if (content.GetLength(0) != content.GetLength(1))
        {
            Debug.LogWarning("Warning wront tile format input !!");
        }
        size = content.GetLength(0);
        tileContent = content;
    }

    public int getCenter()
    {
        return tileContent[Mathf.FloorToInt(size/2), Mathf.FloorToInt(size / 2)];
    }

    public static bool operator ==(WaveTile2D wt1, WaveTile2D wt2)
    {
        if (wt1.size != wt2.size)
        {
            return false;
        }
        for (int x = 0; x < wt1.size; x++)
        {
            for (int y = 0; y < wt1.size; y++)
            {
                if (wt1.tileContent[x, y] != wt2.tileContent[x, y])
                    return false;
            }
        } 
        return true;
    }

    public static bool operator !=(WaveTile2D wt1, WaveTile2D wt2)
    {
        return !(wt1 == wt2);
    }
}

[RequireComponent(typeof(SpriteCreator))]
public class WaveFunction2DComponent : MonoBehaviour
{
    [SerializeField]
    private GameObject GOInput;
    private WaveFunctionGrid2D currentGrid;
    private WaveFunctionGrid2D inputGrid;
    // Start is called before the first frame update
    void Start()
    {
        currentGrid = GetComponent<SpriteCreator>().getGrid();
        inputGrid = GOInput.GetComponent<SpriteCreator>().getGrid();
        
        currentGrid.setColors(GOInput.GetComponent<SpriteCreator>().getColors());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void launchWaveFunction()
    {
        Debug.Log("Grid Function Launch");
        currentGrid.launchWaveFunction(inputGrid);
        
    }
}
