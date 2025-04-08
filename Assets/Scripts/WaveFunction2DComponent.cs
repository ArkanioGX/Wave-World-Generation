using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;




public class WaveFunctionGrid2D
{
    public const int TILESIZE = 3;

    public UnityEvent updateSpriteEvt;

    public bool isCreatedProperly = false;
    public bool hasBeenModified = true;
    public bool isXClamped = false;
    public bool isYClamped = true;
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
        private int lastSize;

        public EntropyTile(EntropyTile et)
        {
            compatibleList = new List<WaveTile2D>(et.compatibleList);
            pos = et.pos;
            isTileDone = et.isTileDone;
            lastSize = et.lastSize;
        }

        public List<int> getEntropyList()
        {
            List<int> IDPossible = new List<int>();
            foreach (WaveTile2D wt in compatibleList)
            {
                int currID = wt.getCenterContent();
                if (!IDPossible.Contains(currID))
                {
                    IDPossible.Add(currID);
                }
            }
            return IDPossible;
        }

        public int getEntropy()
        { 
            return getEntropyList().Count;
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
        Fill(0, true);
        updateSprite();
    }
    public Sprite GetSprite()
    {
        return sprite;
    }
    
    public Color getColorFromEntropy(Vector2Int pos, bool EntropySize)
    {
        if (EntropySize)
        {
            if (entropyTiles == null) { return Color.black; }
            int currentCount = entropyTiles[pos].compatibleList.Count;

            if (currentCount < 10)
            {

                if (currentCount == 0)
                {
                    Debug.Log("Weird ...");
                    return Color.red;
                }
                return Color.HSVToRGB((1.0f / 360) * (currentCount * 30), 1, 1); ;
            }
            else
            {
                return Color.Lerp(Color.white, Color.black, (currentCount - 10) / 10.0f);
            }
        }
        else
        {
            Color color = Color.black;
            int colorN = 0;
            foreach (int cID in entropyTiles[pos].getEntropyList())
            {
                color += colors[cID];
                colorN++;
            }
            return color/colorN;
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
        if (newID == -2) { texture.SetPixel(x, y, getColorFromEntropy(new Vector2Int(x,y),false)); }
        if (newID == -3 ||newID == -1) { texture.SetPixel(x, y, Color.red); }
        updateSpriteEvt.Invoke();
    }
    public void Fill(int id, bool ApplyColor)
    {
        if (ApplyColor) {
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    SetPixelAt(x, y, id);
                }
            }
        }
        else
        {
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    gridContent[x, y] = id;
                }
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
    public int GetTileAt(ref Vector2Int pos)
    {
        if ((pos.x < 0 || pos.x >= size.x) && isXClamped) { return -1; }
        if ((pos.y < 0 || pos.y >= size.y) && isYClamped) { return -1; }
        pos += size;
        pos = new Vector2Int(pos.x%size.x, pos.y % size.y);
        return gridContent[pos.x, pos.y];
    }
    public void ComputeBorderClamp()
    {
        
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
                        Vector2Int newPos = new Vector2Int(x + i, y + j);
                        tempGrid[max+i,max+j] = inputGrid.GetTileAt(ref newPos);
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
        //Debug.Log(tiles.Count);
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
                        Vector2Int newPos = new Vector2Int(x + i, y + j);
                        tempGrid[max + i, max + j] = GetTileAt(ref newPos);
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
                posToDo.Add(new Vector2Int(x, y));
            }
        }
        //ComputeEntropyFromSurrounding();
    }
    public void ComputeEntropyFromSurrounding()
    {
        foreach (KeyValuePair<Vector2Int, EntropyTile> entry in entropyTiles)
        {
            int max = TILESIZE / 2;
            int min = -max;

            List<int> toRemove = new List<int>();

            foreach (WaveTile2D tile in entry.Value.compatibleList)
            {
                bool isAllCompatible = true;
                for (int x = min; x <= max; x++)
                {
                    for (int y = min; y <= max; y++)
                    {

                        Vector2Int localPos = new Vector2Int(x, y);
                        Vector2Int globalPos = entry.Key + localPos;

                        if (GetTileAt(ref globalPos) == -1) { continue; }

                        bool hasFoundOneCompatibility = false;
                        foreach (WaveTile2D tileSurround in entropyTiles[globalPos].compatibleList)
                        {
                            if (WaveTile2D.CheckCompatibility(tile, tileSurround, localPos)) { 
                                hasFoundOneCompatibility = true;
                                continue;
                            }
                        }
                        if (!hasFoundOneCompatibility) 
                        { 
                            isAllCompatible = false;
                            break;
                        }
                    }
                    if (!isAllCompatible) { break; }
                }
            }
        }
    }
    private EntropyTile GetLowestEntropyTile()
    {
        EntropyTile lowest = new EntropyTile();
        int minDist = tiles.Count;
        foreach (Vector2Int pos in posToDo)
        {
            EntropyTile currTile = entropyTiles[pos];
            if (currTile.getEntropy() < minDist)
            {
                lowest = currTile;
                minDist = currTile.getEntropy();
            }
        }
        return lowest;
    }
    public void Generate(WaveFunctionGrid2D inputGrid)
    {
        tiles = new List<WaveTile2D>();
        posToDo = new List<Vector2Int>();
        entropyTiles = new Dictionary<Vector2Int, EntropyTile>();

        //Fill the grid with Any Tile without changing color
        Fill(-2,false);

        //Fill the infos needed for generation
        ComputeWaveTile(inputGrid);
        ComputeEntropyFromTiles();
        

        //Get new colors
        colors = inputGrid.colors.Clone() as Color[];

        //Fill the grid with Any Tile but update color this time
        Fill(-2,true);
    }

    public void Reset()
    {
        posToDo = new List<Vector2Int>();
        entropyTiles = new Dictionary<Vector2Int, EntropyTile>();

        //Fill the grid with Any Tile without changing color
        Fill(-2, false);

        //Fill the infos needed for generation
        ComputeEntropyFromTiles();

        //Fill the grid with Any Tile but update color this time
        Fill(-2, true);
    }

    public void Step()
    {
        if (posToDo.Count > 0)
        {
            EntropyTile lowestEntropyTile = GetLowestEntropyTile();
            Vector2Int currentPos = lowestEntropyTile.pos;

            //Debug.Log("Pos at : " + currentPos);

            if (lowestEntropyTile.compatibleList.Count == 0)
            {
                SetPixelAt(currentPos.x, currentPos.y, -3);
                posToDo.Remove(new Vector2Int(currentPos.x, currentPos.y));
                Reset();
                return;
            }

            WaveTile2D selectedWT = lowestEntropyTile.compatibleList[UnityEngine.Random.Range(0, lowestEntropyTile.compatibleList.Count)];
            SetPixelAt(currentPos.x, currentPos.y, selectedWT.getCenterContent());
            List<WaveTile2D> toRemoveFromCurrent = new List<WaveTile2D>();
            foreach (WaveTile2D currTile in lowestEntropyTile.compatibleList)
            {
                if (currTile.getCenterContent() != selectedWT.getCenterContent())
                {
                    toRemoveFromCurrent.Add(currTile);
                }
            }
            foreach (WaveTile2D currTile in toRemoveFromCurrent)
            {
                lowestEntropyTile.compatibleList.Remove(currTile);
            }
            toRemoveFromCurrent.Clear();

            //Way too slow
            //ComputeEntropyFromSurrounding();

            //Remove unusable wave tile
            int max = TILESIZE / 2;
            int min = -max;
            for (int x = min; x <= max; x++)
            {
                for (int y = min; y <= max; y++)
                {
                    Vector2Int pos = new Vector2Int(currentPos.x+x, currentPos.y + y);
                    if (GetTileAt(ref pos) == -1 || (x == 0 && y == 0)) { continue; }
                    EntropyTile tileCheck = entropyTiles[pos];

                    List<WaveTile2D> toRemove = new List<WaveTile2D>();

                    foreach (WaveTile2D wtcheck in tileCheck.compatibleList)
                    {
                        bool breakWT = false;
                        for (int i = min; i <= max; i++)
                        {
                            for (int j = min; j <= max; j++)
                            {
                                Vector2Int globalPos = new Vector2Int(pos.x+i, pos.y + j);
                                if (GetTileAt(ref globalPos) == -1 || (i == 0 && j == 0)) { continue; }
                                EntropyTile tileGlobalCheck = entropyTiles[globalPos];
                                breakWT = false;
                                foreach (WaveTile2D wtglobalCheck in tileGlobalCheck.compatibleList)
                                {
                                    if (WaveTile2D.CheckCompatibility(wtcheck,wtglobalCheck,new Vector2Int(i,j))) { breakWT = true; break; }
                                }
                                if (!breakWT) break;
                            }
                            if (!breakWT) break;
                        }
                        if (!breakWT) toRemove.Add(wtcheck);
                    }

                    
                    Debug.Log(toRemove.Count);
                    while (toRemove.Count > 0)
                    {
                        tileCheck.compatibleList.Remove(toRemove[0]);
                        toRemove.RemoveAt(0);
                        
                    }
                    //Refresh Entropy Colors
                    if (GetTileAt(ref pos) == -2) { SetPixelAt(pos.x, pos.y, -2); }
                }
            }

            

            //Remove the pos
            posToDo.Remove(new Vector2Int(currentPos.x, currentPos.y));
        }
        else
        {
            Debug.Log("The grid should be filled");
        }
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
                if (wt1.tileContent[x,y] != wt2.tileContent[x-offset.x, y - offset.y]) { return false; }
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
        if (wt == null || wt.size != size) { return false; }
        for (int x = 0; x < wt.size; x++)
        {
            for (int y = 0; y < wt.size; y++)
            {
                //Then if in the current tile its required to be a border it will not be compatible 
                if (wt.tileContent[x, y] != tileContent[x, y]) { return false; }
            }
        }
        return true;
    }
    public override bool Equals(object obj) => Equals(obj as WaveTile2D);

    public override int GetHashCode()
    {
        unchecked
        {
            return tileContent.GetHashCode(); ;
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

    bool isToggled = false;
    bool isHeld = false;

    public InputActionAsset actions;

    public bool doTileRotateOrMirror = false;

    private void Start()
    {
        actions.Enable();
        actions.FindActionMap("Generation").FindAction("Step").performed += OnStep;
        actions.FindActionMap("Generation").FindAction("Hold").performed += OnHoldDown;
        actions.FindActionMap("Generation").FindAction("ReleaseHold").performed += OnHoldUp;
        actions.FindActionMap("Generation").FindAction("Toggle").performed += OnToggle;
    }

    public void LaunchFunction()
    {
        currentGrid = GetComponent<SpriteCreator>().grid;
        inputGrid = GOInput.GetComponent<SpriteCreator>().grid;

        currentGrid.Generate(inputGrid);
    }

    private void Update()
    {
        GetComponent<SpriteCreator>().ApplyChanges();
        if ((isToggled ||isHeld) && currentGrid != null)
        {
            currentGrid.Step();
        }
    }

    private void OnStep(InputAction.CallbackContext context)
    {
        if (currentGrid != null)
        {
            currentGrid.Step();
        }
    }

    private void OnHoldDown(InputAction.CallbackContext context)
    {
        isHeld = true;
    }

    private void OnHoldUp(InputAction.CallbackContext context)
    {
        isHeld = false;
    }

    private void OnToggle(InputAction.CallbackContext context)
    {
        isToggled = !isToggled;
    }
}
