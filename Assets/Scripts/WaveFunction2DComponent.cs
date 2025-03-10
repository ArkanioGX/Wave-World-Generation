using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
using UnityEngine.Windows;



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

    List<EntropyTile> posToDo;
    Dictionary<int, List<WaveTile2D>> tiles;

    private struct EntropyTile
    {
        public List<int> compatibleList;
        public Vector2Int pos;
        

    }

    public WaveFunctionGrid2D( Vector2Int newSize, Color[] newColors)
    {
        colors = newColors;
        size = newSize;
        updateSpriteEvt = new UnityEvent();
        init();
    }

    private int[,] getGroupAt(Vector2Int center, int size)
    {
        int[,] group = new int[size,size];
        int min = -Mathf.FloorToInt(TILESIZE / 2);
        int max = Mathf.FloorToInt(TILESIZE / 2);
        for (int i = min; i <= max; i++)
        {
            for (int j = min; j <= max; j++)
            {
                group[max + i, max + j] = GetTileAt(new Vector2Int(center.x + i, center.y + j));
            }
        }
        return group;
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
        posToDo = new List<EntropyTile> ();

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
        int clampedID = Mathf.Clamp(newID, 0, colors.Length - 1);
        x = Mathf.Clamp(x, 0, size.x - 1);
        y = Mathf.Clamp(y, 0, size.y - 1);
        gridContent[x,y] = newID;
        Color color = colors[clampedID];

        texture.SetPixel(x, y, color);
        if (newID == -2) { texture.SetPixel(x, y, Color.red); }
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
        if (pos.x <  0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y ) return -1;
        return gridContent[pos.x, pos.y];
    }

    public void launchWaveFunction(WaveFunctionGrid2D input)
    {
        //Reset grid with Any (-2) Tile
        Fill(-2);
        //Set the colors
        setColors(input.colors);

        tiles = new Dictionary<int, List<WaveTile2D>>();
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
        //Get Random Start Pos
        Vector2Int start = new Vector2Int(UnityEngine.Random.Range(0,size.x-1), UnityEngine.Random.Range(0,size.y-1));

        posToDo = new List<EntropyTile>();
        for (int x = 0; x < gridContent.GetLength(0); x++)
        {
            for (int y = 0; y < gridContent.GetLength(1); y++)
            {
                EntropyTile value = new EntropyTile();
                value.pos = new Vector2Int(x, y);
                value.compatibleList = getListOfCompatible(new Vector2Int(x, y), tiles);
                posToDo.Add(value);
            }
        }
        

    }

    public void update(int n)
    {
        for (int m= 0; m < n; m++)
        {
            if (posToDo.Count > 0)
            {
                //Get the lowest entropy tile
                EntropyTile currentETile = getLowestEntropy(posToDo);
                Vector2Int currentPos = currentETile.pos;
                if (currentETile.compatibleList.Count == 0)
                {
                    removeInListFromPos(ref posToDo, currentPos);
                    continue;
                }

                //Check Compatibility
                int randomID = UnityEngine.Random.Range(0, currentETile.compatibleList.Count);
                SetPixelAt(currentPos.x, currentPos.y, currentETile.compatibleList[randomID]);


                int min = -Mathf.FloorToInt(TILESIZE / 2);
                int max = Mathf.FloorToInt(TILESIZE / 2);

                //Check if compatibility around are possible
                bool isAroundCompatible = true;
                for (int i = min; i <= max; i++)
                {
                    for (int j = min; j <= max; j++)
                    {
                        Vector2Int pos = new Vector2Int(currentPos.x + i, currentPos.y + j);
                        if (GetTileAt(pos) == -1)
                        {
                            continue;
                        }
                        List<int> l = getListOfCompatible(pos, tiles);
                        if (l.Count == 0)
                        {
                            isAroundCompatible = false;
                        }
                    }
                }
                //Update Tile Around
                int o = 0;
                while (!isAroundCompatible && o < currentETile.compatibleList.Count)
                {
                    o++;
                    isAroundCompatible = true;
                    randomID = ((randomID+1) % currentETile.compatibleList.Count);
                    SetPixelAt(currentPos.x, currentPos.y, currentETile.compatibleList[randomID]);
                    for (int i = min; i <= max; i++)
                    {
                        for (int j = min; j <= max; j++)
                        {
                            List<int> l = getListOfCompatible(new Vector2Int(currentPos.x + i, currentPos.y + j), tiles);
                            if (l.Count == 0)
                            {
                                isAroundCompatible = false;
                            }
                            
                        }
                    }
                }

                for (int i = min; i <= max; i++)
                {
                    for (int j = min; j <= max; j++)
                    {
                        RecomputeEntropyTileAtPos(posToDo, new Vector2Int(currentPos.x + i, currentPos.y + j), tiles);
                    }
                }

                //Remove current ETile
                removeInListFromPos(ref posToDo, currentPos);
            }
            
        }
    }

    private List<int> getListOfCompatible(Vector2Int center, Dictionary<int, List<WaveTile2D>> tiles)
    {
        List<int> compatibleList = new List<int>();
        int[,] currentTileInGrid = getGroupAt(center, TILESIZE);
        foreach (KeyValuePair<int, List<WaveTile2D>> wtpair in tiles)
        {
            List<WaveTile2D> wtlist = wtpair.Value;
            foreach (WaveTile2D wt in wtlist)
            {
                if (wt.isCompatible(currentTileInGrid))
                {
                    compatibleList.Add(wtpair.Key);
                    break;
                }
            }
        }
        return compatibleList;
    }

    private EntropyTile getLowestEntropy(List<EntropyTile> list)
    {
        EntropyTile result = list[0];
        foreach (EntropyTile t in list)
        {
            if (t.compatibleList.Count < result.compatibleList.Count)
            {
                result = t;
            }
        }
        return result;
    }

    private void RecomputeEntropyTileAtPos(List<EntropyTile> list, Vector2Int pos, Dictionary<int, List<WaveTile2D>> tiles)
    {
        for(int i = 0; i < list.Count; i++)
        {
            EntropyTile t = list[i];
            if (t.pos == pos)
            {
                t.compatibleList = getListOfCompatible(pos, tiles);
                list[i] = t;
                return;
            }
        }
    }

    private void removeInListFromPos(ref List<EntropyTile> list, Vector2Int pos)
    {
        for (int i = 0; i < list.Count; i++)
        {
            EntropyTile t = list[i];
            if (t.pos == pos)
            {
               list.RemoveAt(i);
                return;
            }
        }
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
        return tileContent[Mathf.FloorToInt(size / 2), Mathf.FloorToInt(size / 2)];
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

    public override bool Equals(object obj)
    {
        
        if (obj is WaveTile2D)
        {
            WaveTile2D wt = (WaveTile2D)obj;
            if (size != wt.size)
            {
                return false;
            }
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (tileContent[x, y] != wt.tileContent[x, y])
                        return false;
                }
            }
            return true;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(size,tileContent);
    }

    public bool isCompatible(int[,] contentToCheck)
    {
        if (size != contentToCheck.GetLength(0) || contentToCheck.GetLength(0) != contentToCheck.GetLength(1))
        {
            return false;
        }
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                //If Any Tile check if the current wavetile has a border wall at the same place (Border and Any is incompatible)
                if (contentToCheck[x,y] == -2)
                {
                    if (tileContent[x,y] == -1)
                    {
                        return false;
                    }
                }
                else
                {
                    if (contentToCheck[x, y] != tileContent[x, y])
                    {
                        return false;
                    }
                }
            }
        }
        return true;
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
        currentGrid.updateSpriteEvt.AddListener(updateSprite);
    }

    // Update is called once per frame
    void Update()
    {
        currentGrid.update(1);
    }

    public void updateSprite()
    {
        if (currentGrid.updateSprite())
        {
            GetComponent<SpriteRenderer>().sprite = currentGrid.GetSprite();
        }
    }

    public void launchWaveFunction()
    {
        Debug.Log("Grid Function Launch");
        currentGrid.launchWaveFunction(inputGrid);
        if (currentGrid.updateSprite())
        {
            GetComponent<SpriteRenderer>().sprite = currentGrid.GetSprite();
        }
    }
}
