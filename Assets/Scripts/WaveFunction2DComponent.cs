using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;



public class WaveFunctionGrid2D
{
    public const int TILESIZE = 3;

    public UnityEvent updateSpriteEvt;

    public bool isCreatedProperly = false;
    public bool hasBeenModified = true;
    public int[,] gridContent;
    private Texture2D texture;
    private Sprite sprite;
    public Vector2Int size;
    public Color[] colors;
    public int maxID = 10;

    public bool tileRotMirror = false;

    List<Vector2Int> posToDo;
    Dictionary<Vector2Int,EntropyTile> entropyTiles;
    List<WaveTile2D> tiles;

    private struct EntropyTile
    {
        public List<WaveTile2D> compatibleList;
        public Vector2Int pos;
        public bool isTileDone;

        public EntropyTile(EntropyTile et)
        {
            compatibleList = new List<WaveTile2D>(et.compatibleList);
            pos = et.pos;
            isTileDone = et.isTileDone;
        }

    }

    public WaveFunctionGrid2D(Vector2Int newSize, Color[] newColors)
    {
        colors = newColors;
        size = newSize;
        updateSpriteEvt = new UnityEvent();
        init();
    }

    public WaveFunctionGrid2D(Sprite sprt)
    {
        updateSpriteEvt = new UnityEvent();
        getContentFromSprite(sprt);
        posToDo = new List<Vector2Int>();
        entropyTiles = new Dictionary<Vector2Int, EntropyTile>();
        maxID = colors.Length;


    }

    private void getContentFromSprite(Sprite sprt)
    {
        texture = sprt.texture;
        size = new Vector2Int(texture.width, texture.height);

        gridContent = new int[size.x, size.y];
        List<Color> colorsInTexture = new List<Color>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Color currentCol = texture.GetPixel(x, y);
                int id = 0;
                for (id = 0; id < colorsInTexture.Count; id++)
                {
                    if (currentCol == colorsInTexture[id])
                    {
                        gridContent[x, y] = id;
                        break;
                    }
                }
                if (id == colorsInTexture.Count)
                {
                    colorsInTexture.Add(currentCol);
                    gridContent[x, y] = id;
                }

            }
        }
        colors = colorsInTexture.ToArray();
    }
    private void refreshColor()
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                SetPixelAt(x, y, gridContent[x, y]);
            }
        }
    }
    public void setColors(Color[] newColors)
    {
        colors = newColors;
        refreshColor();
    }
    public void init()
    {
        texture = new Texture2D(size.x, size.y);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        gridContent = new int[size.x, size.y];
        entropyTiles = new Dictionary<Vector2Int, EntropyTile>();
        posToDo = new List<Vector2Int>();
        maxID = colors.Length;

        Fill(0);
        updateSprite();
    }
    public Sprite GetSprite()
    {
        return sprite;
    }
    
    public Color getColorFromEntropy(Vector2Int pos)
    {
        int currentCount = entropyTiles[pos].compatibleList.Count;
        if (currentCount < 10) {
            Color[] cList =
            {
                new Color(0.9f,0.1f,0.1f,1.0f),
                new Color(1.0f,0.5f,0.1f,1.0f),
                new Color(1.0f,0.8f,0.0f,1.0f),
                new Color(1.0f,1.0f,0.1f,1.0f),
                new Color(0.9f,0.8f,0.1f,1.0f),
                new Color(0.1f,0.7f,0.3f,1.0f),
                new Color(0.7f,0.9f,0.1f,1.0f),
                new Color(0.0f,0.6f,0.9f,1.0f),
                new Color(0.6f,0.8f,0.9f,1.0f),
                new Color(0.2f,0.2f,0.8f,1.0f)
            };
            return cList[currentCount];
        }
        else if (currentCount < 20)
        {
            return Color.Lerp(new Color(0.6f,0.3f,0.6f,1.0f), new Color(0.8f, 0.7f, 0.9f, 1.0f), (currentCount - 10) / 10.0f);
        }
        else
        {
            return Color.Lerp(new Color(1,1,1,1), new Color(0,0,0,1), (currentCount - 20) / 30.0f);
        }
    }
    public void SetPixelAt(int x, int y, int newID)
    {
        hasBeenModified = true;
        int clampedID = Mathf.Clamp(newID, 0, colors.Length - 1);
        x = Mathf.Clamp(x, 0, size.x - 1);
        y = Mathf.Clamp(y, 0, size.y - 1);
        gridContent[x, y] = newID;
        Color color = colors[clampedID];

        texture.SetPixel(x, y, color);
        if (newID == -2) { texture.SetPixel(x, y, getColorFromEntropy(new Vector2Int(x, y))); }
        if (newID == -3 ||newID == -1) { texture.SetPixel(x, y, Color.red); }
        updateSpriteEvt.Invoke();
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
        if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y) return -1;
        return gridContent[pos.x, pos.y];
    }
    public void ComputeWaveTile(WaveFunctionGrid2D inputGrid)
    {
        
        for (int x = 0; x < inputGrid.size.x; x++)
        {
            for (int y = 0; y < inputGrid.size.y; y++)
            {
                int[,] tempGrid = new int[TILESIZE,TILESIZE];
                int max = TILESIZE/2;
                int min = -max;

                //Fill the grid from the input
                for (int i = min; i <= max; i++)
                {
                    for (int j = min; j <= max; j++)
                    {
                        tempGrid[max+i,max+j] = inputGrid.GetTileAt(new Vector2Int(x+i,y+j));
                    }
                }

                //Create wavetile from the grid
                WaveTile2D waveTile2D = new WaveTile2D(tempGrid);

                if (!tiles.Contains(waveTile2D))
                {
                    //Add to the list
                    tiles.Add(waveTile2D);
                }
            }
        }
        Debug.Log(tiles.Count);
    }

    public void ComputeEntropyFromTiles()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y ; y++)
            {
                EntropyTile et = new EntropyTile();
                et.isTileDone = false;
                et.pos = new Vector2Int(x,y);

                //Compatible Creation
                List<WaveTile2D> cList = new List<WaveTile2D>();
                int[,] tempGrid = new int[TILESIZE, TILESIZE];
                int max = TILESIZE / 2;
                int min = -max;

                //Fill the grid from the input
                for (int i = min; i <= max; i++)
                {
                    for (int j = min; j <= max; j++)
                    {
                        tempGrid[max + i, max + j] = GetTileAt(new Vector2Int(x + i, y + j));
                    }
                }

                foreach (WaveTile2D tile in tiles)
                {
                    if (WaveTile2D.CheckCompatibility(tile, tempGrid))
                    {
                        cList.Add(tile);
                    }
                }

                et.compatibleList = new List<WaveTile2D>(cList);
                entropyTiles.Add(new Vector2Int(x,y),new EntropyTile(et));
            }
        }
    }

    public void Generate(WaveFunctionGrid2D inputGrid)
    {
        tiles = new List<WaveTile2D>();
        posToDo = new List<Vector2Int>();
        entropyTiles = new Dictionary<Vector2Int, EntropyTile>();

        ComputeWaveTile(inputGrid);
        ComputeEntropyFromTiles();

        //Get new colors
        colors = inputGrid.colors.Clone() as Color[];

        //Fill the grid with Any Tile
        Fill(-2);


    }
    
}

public class WaveTile2D
{
    public int size;
    public int[,] tileContent;

    // -1 = Image Border

    // -2 = Any Tile

    // -3 = Error Tile

    public WaveTile2D(int[,] newContent)
    {
        //Check that both sides size are equal
        Debug.Assert(newContent.GetLength(0) == newContent.GetLength(1), "Wrong Size input in this Wavetile");

        size = newContent.GetLength(0);
        tileContent = newContent.Clone() as int[,];
    }

    public WaveTile2D(WaveTile2D wt)
    {
        size = wt.size;
        tileContent = wt.tileContent.Clone() as int[,];   
    }

    public static bool CheckCompatibility(WaveTile2D wt, int[,] content)
    {
        if (wt == null || wt.size != content.GetLength(0) || wt.size != content.GetLength(1)) { return false; }
        for (int x = 0; x < wt.size; x++)
        {
            for (int y = 0; y < wt.size; y++)
            {
                //Check if any tile
                if (content[x,y] == -2)
                {
                    //Then if in the current tile its required to be a border it will not be compatible 
                    if (wt.tileContent[x, y] == -1) return false;
                }
                else
                {
                    //Then if in the current tile its required to be a border it will not be compatible 
                    if (wt.tileContent[x, y] != content[x, y]) { return false; }
                }
            }
        }
        return true;
    }

    public static bool CheckCompatibility(WaveTile2D wt1, WaveTile2D wt2, Vector2Int offset )
    {
        if (wt1 == null || wt2 == null || wt1.size != wt2.size) { return false;}
        for (int x = Mathf.Max(0,offset.x); x < Mathf.Min(wt1.size, wt1.size+offset.x); x++)
        {
            for (int y = Mathf.Max(0, offset.y); y < Mathf.Min(wt1.size, wt1.size + offset.y); y++)
            {
                if (wt1.tileContent[x,y] != wt1.tileContent[x-offset.x, y - offset.y]) { return false; }
            }
        }
        return true;
    }

    //Get the center in the grid 
    public Vector2Int getCenterPos()
    {
        int c = Mathf.RoundToInt(size/2);
        return new Vector2Int(c,c);
    }

    //Get the content of the center of the grid
    public int getCenterContent()
    {
        Vector2Int c =getCenterPos();
        return tileContent[c.x,c.y];
    }

    public static bool operator ==(WaveTile2D wt1, WaveTile2D wt2)
    {
        if (ReferenceEquals(wt1, wt2))
            return true;
        if (ReferenceEquals(wt1, null))
            return false;
        if (ReferenceEquals(wt2, null))
            return false;
        return wt1.Equals(wt2);
    }
    public static bool operator !=(WaveTile2D wt1, WaveTile2D wt2) => !( wt1 == wt2);

    public bool Equals(WaveTile2D wt)
    {
        if (ReferenceEquals(wt, null))
            return false;
        if (ReferenceEquals(this, wt))
            return true;
        if (GetHashCode() == wt.GetHashCode())
            return true;
        return false;
    }
    public override bool Equals(object obj) => Equals(obj as WaveTile2D);

    public override int GetHashCode()
    {
        unchecked
        {
            int n = 0;
            int hashCode = 0;
            foreach (int i in tileContent)
            {
                //+1 is to count for the wall/border tile
                hashCode += (i+1) * (1 << n);
                n++;
            }
            return hashCode;
        }
    }

    /// <summary>
    /// Rotate by 90 * n degrees the the waveTile
    /// </summary>
    public void Rotate(double n)
    {
        n *= Mathf.Deg2Rad * 90;
        int[,] temp = tileContent.Clone() as int[,];
        Vector2Int center = new Vector2Int(size / 2, size / 2);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (pos != center)
                {
                    int newX = center.x + Mathf.RoundToInt((float)((x-center.x)*Math.Cos(n) - (y - center.y) * (float)Math.Sin(n)));
                    int newY = center.y + Mathf.RoundToInt((float)((y - center.y) *Math.Cos(n) + (x - center.x) * (float)Math.Sin(n)));

                    temp[newX, newY] = tileContent[x, y];
                }
            }
        }
        tileContent = temp.Clone() as int[,];
    }

    /// <summary>
    /// Flip the wavetile in the X axis or Y axis or Both
    /// </summary>
    /// <param name="flipx"> flip on the x side </param>
    /// <param name="flipy"> flip on the y side </param>
    public void Mirror(bool flipx, bool flipy)
    {
        int[,] temp = tileContent.Clone() as int[,];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int newX = flipx ? (size-1)-x : x;
                int newY = flipy ? (size-1)-y : y;

                temp[newX, newY] = tileContent[x, y];
            }
        }
        tileContent = temp.Clone() as int[,];
    }

}

[RequireComponent(typeof(SpriteCreator))]
public class WaveFunction2DComponent : MonoBehaviour
{
    [SerializeField]
    private GameObject GOInput;
    private WaveFunctionGrid2D currentGrid;
    private WaveFunctionGrid2D inputGrid;

    public bool doTileRotateOrMirror = false;

    public void LaunchFunction()
    {
        currentGrid = GetComponent<SpriteCreator>().grid;
        inputGrid = GOInput.GetComponent<SpriteCreator>().grid;

        currentGrid.Generate(inputGrid);
    }

    private void Update()
    {
        GetComponent<SpriteCreator>().ApplyChanges();
    }
}
