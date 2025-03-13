using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;



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

    public WaveTile2D lastWaveTile;

    public bool tileRotMirror = false;

    List<Vector2Int> posToDo;
    Dictionary<Vector2Int,EntropyTile> entropyTiles;
    List<WaveTile2D> tiles;
    bool forceStop = false;

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

    private int[,] getGroupAt(Vector2Int center, int size)
    {
        int[,] group = new int[size, size];
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

    public void SetPixelAt(int x, int y, int newID)
    {
        hasBeenModified = true;
        int clampedID = Mathf.Clamp(newID, 0, colors.Length - 1);
        x = Mathf.Clamp(x, 0, size.x - 1);
        y = Mathf.Clamp(y, 0, size.y - 1);
        gridContent[x, y] = newID;
        Color color = colors[clampedID];

        texture.SetPixel(x, y, color);
        if (newID == -2) { texture.SetPixel(x, y, mixCompatibleColor(x, y)); }
        else if (newID == -3 ||newID == -1) { texture.SetPixel(x, y, Color.red); }
        updateSpriteEvt.Invoke();
    }

    private Color mixCompatibleColor(int x, int y)
    {
        List<int> idList = GetIntCompatible(getListOfCompatible(new Vector2Int(x, y), tiles));
        int colorN = idList.Count;
        Color c = Color.black;
        foreach (int id in idList)
        {
            c += colors[id];
        }
        return (c / colorN);
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

    public void launchWaveFunction(WaveFunctionGrid2D input)
    {

        //Set the colors
        setColors(input.colors);

        tiles = new List<WaveTile2D>();

        Vector2 inputSize = input.getSize();
        for (int x = 0; x < input.gridContent.GetLength(0); x++)
        {
            for (int y = 0; y < input.gridContent.GetLength(1); y++)
            {
                int[,] tileContent = new int[TILESIZE, TILESIZE];
                int min = -Mathf.FloorToInt(TILESIZE / 2);
                int max = Mathf.FloorToInt(TILESIZE / 2);

                for (int i = min; i <= max; i++)
                {
                    for (int j = min; j <= max; j++)
                    {
                        tileContent[i - min, j - min] = input.GetTileAt(new Vector2Int(x + i, y + j));
                    }
                }
                WaveTile2D currentTile = new WaveTile2D(tileContent);
                bool existInList = isCurrentTileInList(currentTile);
                //Check if it exist already in the list

                if (!existInList)
                {

                    tiles.Add(currentTile);
                    if (tileRotMirror)
                    {
                        //Rotate by 90°
                        WaveTile2D nTile = new WaveTile2D(tileContent);
                        nTile.Rotate(1);
                        if (!isCurrentTileInList(nTile)) { tiles.Add(nTile); }


                        //Rotate by 270°
                        nTile = new WaveTile2D(tileContent);
                        nTile.Rotate(3);
                        if (!isCurrentTileInList(nTile)) { tiles.Add(nTile); }

                        //Mirror in X
                        nTile = new WaveTile2D(tileContent);
                        nTile.Mirror(true, false);
                        if (!isCurrentTileInList(nTile)) { tiles.Add(nTile); }

                        //Mirror in Y
                        nTile = new WaveTile2D(tileContent);
                        nTile.Mirror(false, true);
                        if (!isCurrentTileInList(nTile)) { tiles.Add(nTile); }

                        //Mirror in X and Y (same as rotate by 180°)
                        nTile = new WaveTile2D(tileContent);
                        nTile.Mirror(true, true);
                        if (!isCurrentTileInList(nTile)) { tiles.Add(nTile); }
                    }

                }
            }
        }

        //Reset grid with Any (-2) Tile
        Fill(-2);

        //Remove force stop
        forceStop = false;

        //Get Random Start Pos
        Vector2Int start = new Vector2Int(UnityEngine.Random.Range(0, size.x - 1), UnityEngine.Random.Range(0, size.y - 1));

        posToDo = new List<Vector2Int>();
        entropyTiles = new Dictionary<Vector2Int, EntropyTile>();
        for (int x = 0; x < gridContent.GetLength(0); x++)
        {
            for (int y = 0; y < gridContent.GetLength(1); y++)
            {
                EntropyTile value = new EntropyTile();
                value.pos = new Vector2Int(x, y);
                value.compatibleList = getListOfCompatible(new Vector2Int(x, y), tiles);
                value.isTileDone = false;
                posToDo.Add(value.pos);
                entropyTiles.Add(value.pos,value);
            }
        }
        RecomputeAll(entropyTiles,tiles);

        

    }

    public bool isCurrentTileInList(WaveTile2D currentTile)
    {
        foreach (WaveTile2D tile in tiles)
        {
            if (tile == currentTile)
            {
                return true;
            }
        }
        return false;
    }

    public void update(int n)
    {
        for (int m= 0; m < n; m++)
        {
            if (posToDo.Count > 0 && !forceStop)
            {
                //Get the lowest entropy tile
                EntropyTile currentETile =  getLowestEntropy();
                Vector2Int currentPos = currentETile.pos;

                if (currentETile.compatibleList.Count == 0)
                {
                    posToDo.Remove(currentPos);
                    currentETile.isTileDone = true;
                    continue;
                }

                
                int randomID = UnityEngine.Random.Range(0, currentETile.compatibleList.Count);
                bool isCompatible = true;
                int count = -1;
                //Check Compatibility
                do
                {
                    
                    List<EntropyTile> surroundingTiles = new List<EntropyTile>();
                    count++;
                    int min = -Mathf.FloorToInt((TILESIZE) / 2);
                    int max = Mathf.FloorToInt((TILESIZE) / 2);
                    for (int y = min; y <= max; y++)
                    {
                        for (int x = min; x <= max; x++)
                        {
                            if (GetTileAt(new Vector2Int(currentPos.x+x,currentPos.y+y)) == -1) { continue; }
                            surroundingTiles.Add(new EntropyTile(entropyTiles[new Vector2Int(currentPos.x + x, currentPos.y + y)]));
                        }
                    }


                    
                }
                while (!isCompatible && count< currentETile.compatibleList.Count);


                
                
                //SpriteDebug.Instance().setColors(colors);
                //SpriteDebug.Instance().drawWaveTile(1);
                int newID = 1;
                SetPixelAt(currentPos.x, currentPos.y, newID);

                

                //Remove current ETile
                currentETile.isTileDone = true;
                posToDo.Remove(currentPos);
            }
            
        }
        
    }

    private List<WaveTile2D> getLocalListOfCompatible(List<WaveTile2D> t, int[,] grid)
    {
        List<WaveTile2D> compatibleList = new List<WaveTile2D>();
        foreach (WaveTile2D wt in t)
        {
            if (wt.isCompatible(grid))
            {
                compatibleList.Add(wt);
            }
        }
        return compatibleList;
    }

    private List<WaveTile2D> getListOfCompatible(Vector2Int center, List<WaveTile2D> t)
    {
        List<WaveTile2D> compatibleList = new List<WaveTile2D>();
        int[,] currentTileInGrid = getGroupAt(center, TILESIZE);
        foreach (WaveTile2D wt in t)
        {
            if (wt.isCompatible(currentTileInGrid))
            {
                compatibleList.Add(wt);
            }
        }
        return compatibleList;
    }

    private List<int> GetIntCompatible(List<WaveTile2D> l)
    {
        
        List<int> compatibleList = new List<int>();
        foreach (WaveTile2D wt in l)
        {
            bool isInsideList = false;
            int currentCenter = wt.getCenter();
            for (int i = 0; i < compatibleList.Count; i++)
            {
                if (currentCenter == compatibleList[i])
                {
                    isInsideList = true;
                    break;
                }
            }
            if (!isInsideList)
            {
                compatibleList.Add(currentCenter);
            }
        }
        return compatibleList;
    }

    private EntropyTile getLowestEntropy()
    {
        EntropyTile result = new EntropyTile();
        int sizeMin = 99999999;
        foreach (Vector2Int pos in posToDo)
        {
            EntropyTile t = entropyTiles[pos];
            if (t.compatibleList.Count < sizeMin)
            {
                result = t;
                sizeMin = t.compatibleList.Count;
            }
        }
        return result;
    }

    private void RecomputeEntropyTileAtPos(Dictionary<Vector2Int, EntropyTile> list, Vector2Int pos)
    {
        if (GetTileAt(pos) == -1) { return; }
        EntropyTile et = list[new Vector2Int(pos.x, pos.y)];
        
        //TODO: RECOMPUTE COMPATIBILITY
        return;
    }

    private void RecomputeAll(Dictionary<Vector2Int,EntropyTile> list, List<WaveTile2D> lwt)
    {
        foreach (KeyValuePair<Vector2Int, EntropyTile> entry in list)
        {
            EntropyTile t = entry.Value;
            t.compatibleList = getListOfCompatible(t.pos, lwt);
            if (GetTileAt(t.pos) == -2)
            {
                //Refresh Color
                SetPixelAt(t.pos.x, t.pos.y, -2);
            }
        }
        return;
    }

    private bool getCompatibleAround(Vector2Int pos, List<WaveTile2D> lwt)
    {
        return true;
    }
}

public class WaveTile2D
{
    public int size;
    public int[,] tileContent;

    // -1 = Image Border

    // -2 = Any Tile

    // -3 = Error Tile

    public WaveTile2D(int[,] content)
    {
        if (content.GetLength(0) != content.GetLength(1))
        {
            Debug.LogWarning("Warning wront tile format input !!");
        }
        size = content.GetLength(0);
        tileContent = content.Clone() as int[,];
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
                    continue;
                }
                else
                {
                    if (contentToCheck[x, y] != tileContent[x, y])
                    {
                        return false;
                    }
                    continue;
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

    public bool doTileRotateOrMirror = false;
    // Start is called before the first frame update
    void Start()
    {
        currentGrid = GetComponent<SpriteCreator>().getGrid();
        currentGrid.tileRotMirror = doTileRotateOrMirror;
        inputGrid = GOInput.GetComponent<SpriteCreator>().getGrid();
        
        currentGrid.setColors(GOInput.GetComponent<SpriteCreator>().getColors());
        currentGrid.updateSpriteEvt.AddListener(updateSprite);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            currentGrid.update(1);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            currentGrid.update(1);
        }

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
        inputGrid = GOInput.GetComponent<SpriteCreator>().getGrid();
        currentGrid.launchWaveFunction(inputGrid);
        if (currentGrid.updateSprite())
        {
            GetComponent<SpriteRenderer>().sprite = currentGrid.GetSprite();
        }
    }
}
