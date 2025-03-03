using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WaveFunctionGrid2D
{
    public bool isCreatedProperly = false;
    public bool hasBeenModified = true;
    public int[,] gridContent;
    private Texture2D texture;
    private Sprite sprite;
    public Vector2Int size;
    public Color[] colors;

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
}

public class WaveTile2D
{
    public int size;
    public int[,] tileContent;

    public void createArray()
    {
        tileContent = new int[size,size];
    }
}

[RequireComponent(typeof(SpriteCreator))]
public class WaveFunction2DComponent : MonoBehaviour
{
    [SerializeField]
    private GameObject GOInput;
    private WaveFunctionGrid2D newGrid;
    // Start is called before the first frame update
    void Start()
    {
        newGrid = GetComponent<SpriteCreator>().getGrid();
        WaveFunctionGrid2D gridInput = GOInput.GetComponent<SpriteCreator>().getGrid();
        int[,] inputContent = gridInput.getContent();
        if (newGrid == null)
        {
            newGrid = GetComponent<SpriteCreator>().createGrid();
        }
        for (int x = 0; x < inputContent.GetLength(0); x++)
        {
            for (int y = 0; y < inputContent.GetLength(1); y++)
            {
                newGrid.SetPixelAt(x, y, inputContent[x, y]);
            }
        }
        newGrid.setColors(GOInput.GetComponent<SpriteCreator>().getColors());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
