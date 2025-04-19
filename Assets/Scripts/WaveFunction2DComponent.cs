using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;
using Unity.VisualScripting;




public class WaveFunctionGrid2D
{
    public int TILESIZE = 3;
    public int PRECISION = 5; //-1 For best precision possible

    public UnityEvent updateSpriteEvt;

    public bool isCreatedProperly = false;
    public bool hasBeenModified = true;
    public bool isXClamped = true;
    public bool isYClamped = true;
    public bool repeatOnce = true;
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

    Vector2Int lastPosDone;

    private struct EntropyTile
    {
        public List<WaveTile2D> compatibleList;
        public Vector2Int pos;
        public bool hasBeenModified;
        private int lastSize;

        public EntropyTile(EntropyTile et)
        {
            compatibleList = new List<WaveTile2D>(et.compatibleList);
            pos = et.pos;
            hasBeenModified = et.hasBeenModified;
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

    public WaveFunctionGrid2D(Vector2Int newSize, Color[] newColors,int PatternSIZE = 3, int PrecisionP = 5, bool XClamp = false, bool YClamp = false)
    {
        colors = newColors;
        size = newSize;
        TILESIZE = PatternSIZE;
        isXClamped = XClamp;
        isYClamped = YClamp;
        PRECISION = PrecisionP;
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
        if (PRECISION == -1) { PRECISION = Mathf.Min(size.x, size.y) / 2; }
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
            foreach (WaveTile2D wtID in entropyTiles[pos].compatibleList)
            {
                color += colors[wtID.getCenterContent()];
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
            if (sprite == null)
            {
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
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
        pos = new Vector2Int(pos.x % size.x, pos.y % size.y);
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

    private bool CheckDir(Vector2Int centerDiff, Vector2Int localSurroundPos, bool checkInside)
    {
        //Check if its a corner
        if (Mathf.Abs(centerDiff.x) == Mathf.Abs(centerDiff.y))
        {
            centerDiff = new Vector2Int((int)Mathf.Sign(centerDiff.x), (int)Mathf.Sign(centerDiff.y));
            return (Mathf.Abs(localSurroundPos.x) >= Mathf.Abs(centerDiff.x) && Mathf.Sign(localSurroundPos.x) == Mathf.Sign(centerDiff.x) ||
                Mathf.Abs(localSurroundPos.y) >= Mathf.Abs(centerDiff.y) && Mathf.Sign(localSurroundPos.y) == Mathf.Sign(centerDiff.y));
        }
        else
        {
            if (Mathf.Abs(centerDiff.x) < Mathf.Abs(centerDiff.y))
            {
                centerDiff = new Vector2Int(0, (int)Mathf.Sign(centerDiff.y));
                return (Mathf.Abs(localSurroundPos.y) >= Mathf.Abs(centerDiff.y) && Mathf.Sign(localSurroundPos.y) == Mathf.Sign(centerDiff.y));
            }
            else
            {
                centerDiff = new Vector2Int((int)Mathf.Sign(centerDiff.x),0);
                return (Mathf.Abs(localSurroundPos.x) >= Mathf.Abs(centerDiff.x) && Mathf.Sign(localSurroundPos.x) == Mathf.Sign(centerDiff.x));
            }
        }
        
    }

    public void ComputeEntropyFromTiles()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y ; y++)
            {
                EntropyTile et = new EntropyTile();
                et.hasBeenModified = true;
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
                Debug.Assert(cList.Count > 0,"No WaveTile are compatible at this place");

                et.compatibleList = new List<WaveTile2D>(cList);
                entropyTiles.Add(new Vector2Int(x,y),new EntropyTile(et));
                posToDo.Add(new Vector2Int(x, y));
            }
        }
    }

    public void PrecomputeEntropy()
    {
        if (!isXClamped && !isYClamped) { return; }
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int centerPos = new Vector2Int(x, y);
                removeUnusableTileAt(centerPos);
            }
        }
    }

    private void removeUnusableTileAt(Vector2Int centerPos)
    {
        if (GetTileAt(ref centerPos) == -1) { return; }
        EntropyTile centerTile = entropyTiles[centerPos];

        int max = TILESIZE / 2;
        int min = -max;

        Dictionary<Vector2Int, List<WaveTile2D>> toRemove = new Dictionary<Vector2Int, List<WaveTile2D>>();
        toRemove.Add(centerPos, new List<WaveTile2D>());
        //Fill the grid from the input
        foreach (WaveTile2D wtCenter in centerTile.compatibleList)
        {
            bool hasOneCompatible = true;
            for (int i = min; i <= max; i++)
            {
                for (int j = min; j <= max; j++)
                {
                    Vector2Int surroundPos = centerPos + new Vector2Int(i, j);
                    if (GetTileAt(ref surroundPos) == -1 || (i == 0 && j == 0)) { continue; }
                    Vector2Int surrOffset = new Vector2Int(i, j);
                    EntropyTile surroundTile = entropyTiles[surroundPos];
                    if (!surroundTile.hasBeenModified) { continue; }
                    if (!toRemove.ContainsKey(surroundPos)) { toRemove.Add(surroundPos, new List<WaveTile2D>(surroundTile.compatibleList)); }

                    hasOneCompatible = false;
                    foreach (WaveTile2D wtSurround in surroundTile.compatibleList)
                    {
                        if (WaveTile2D.CheckCompatibility(wtCenter, wtSurround, surrOffset))
                        {
                            hasOneCompatible = true;
                            toRemove[surroundPos].Remove(wtSurround);
                        }
                    }
                    if (!hasOneCompatible) { break; }
                }
                if (!hasOneCompatible) { break; }
            }
            if (!hasOneCompatible) { toRemove[centerPos].Add(wtCenter); }
        }

        foreach (KeyValuePair<Vector2Int, List<WaveTile2D>> kvp in toRemove)
        {
            EntropyTile etile = entropyTiles[kvp.Key];
            etile.hasBeenModified = true;
            foreach (WaveTile2D wt in kvp.Value)
            {
                etile.compatibleList.Remove(wt);
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
            int currEntropy = currTile.compatibleList.Count;
            if (currEntropy < minDist)
            {
                lowest = currTile;
                minDist = currEntropy;
            }
        }
        return lowest;
    }

    public void setPrecision(int precisionP)
    {

        PRECISION = precisionP == -1 ? (Mathf.Min(size.x, size.y) / 2)-1 : (Mathf.Min(precisionP, Mathf.Min(size.x, size.y) / 2)-1);
    }
    public void Generate(WaveFunctionGrid2D inputGrid)
    {
        tiles = new List<WaveTile2D>();
        posToDo = new List<Vector2Int>();
        entropyTiles = new Dictionary<Vector2Int, EntropyTile>();

        lastPosDone = Vector2Int.zero;

        //Fill the grid with Any Tile without changing color
        Fill(-2,false);

        //Fill the infos needed for generation
        ComputeWaveTile(inputGrid);
        ComputeEntropyFromTiles();
        PrecomputeEntropy();
        

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
        PrecomputeEntropy();

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
                Debug.LogError("No Compatible Tile found");
                return;
            }

            WaveTile2D selectedWT = lowestEntropyTile.compatibleList[UnityEngine.Random.Range(0, lowestEntropyTile.compatibleList.Count)];
            SetPixelAt(currentPos.x, currentPos.y, selectedWT.getCenterContent());
            lowestEntropyTile.compatibleList.Clear();
            lowestEntropyTile.compatibleList.Add(selectedWT);
            lowestEntropyTile.hasBeenModified = true;

            int max = TILESIZE / 2;
            int min = -max;

            for (int x = min; x <= max; x++)
            {
                for (int y = min; y <= max; y++)
                {
                    Vector2Int pos = new Vector2Int(currentPos.x + x, currentPos.y + y);
                    if (GetTileAt(ref pos) == -1 || (x == 0 && y == 0)) { continue; }
                    EntropyTile tileCheck = entropyTiles[pos];
                    tileCheck.hasBeenModified = true;
                    Vector2Int offset = new Vector2Int(x, y);
                    List<WaveTile2D> toRemove = new List<WaveTile2D>();
                    foreach (WaveTile2D wt in tileCheck.compatibleList)
                    {
                        if (!WaveTile2D.CheckCompatibility(selectedWT, wt, offset))
                        {
                            toRemove.Add(wt);
                        }
                    }
                    foreach (WaveTile2D wt in toRemove)
                    {
                        tileCheck.compatibleList.Remove(wt);
                    }
                }
            }

            //Remove unusable wave tile
            for (int PrecisionN = max+1 ; PrecisionN <= PRECISION; PrecisionN++)
            {
                for (int x = -PrecisionN; x <= PrecisionN; x++)
                {
                    for (int y = -PrecisionN; y <= PrecisionN; y++)
                    {
                        if (!(Mathf.Abs(x) == PrecisionN || Mathf.Abs(y) == PrecisionN)) { continue; }
                        Vector2Int pos = new Vector2Int(currentPos.x + x, currentPos.y + y);
                        removeUnusableTileAt(pos);
                    }
                }
            }
            foreach(Vector2Int pos in posToDo)
            {
                SetPixelAt(pos.x, pos.y,-2);
            }

            foreach (Vector2Int pos in entropyTiles.Keys)
            {
                EntropyTile tile = entropyTiles[pos];
                tile.hasBeenModified = false;
            }
            //Remove the pos
            lastPosDone = currentPos;
            posToDo.Remove(new Vector2Int(currentPos.x, currentPos.y));
        }
        else
        {
            Debug.Log("The grid should be filled");
            //Reset();
        }
    }
    
}

public class WaveTile2D
{
    public int size;
    public int[,] tileContent;
    public Dictionary<Vector2Int,List<WaveTile2D>> Compatible;

    // -1 = Image Border

    // -2 = Any Tile

    // -3 = Error Tile

    public WaveTile2D(int[,] newContent)
    {
        //Check that both sides size are equal
        Debug.Assert(newContent.GetLength(0) == newContent.GetLength(1), "Wrong Size input in this Wavetile");

        size = newContent.GetLength(0);
        tileContent = newContent.Clone() as int[,];
        Compatible = new Dictionary<Vector2Int, List<WaveTile2D>>();
        int max = size / 2;
        int min = -max;
        for (int i = min; i <= max; i++)
        {
            for (int j = min; j <= max; j++)
            {
                Compatible.Add(new Vector2Int(i, j), new List<WaveTile2D>());
            }
        }
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
        if (wt1.Compatible[offset].Contains(wt2)) { return true; }
        if (wt1 == null || wt2 == null || wt1.size != wt2.size) { return false;}
        for (int x = Mathf.Max(0,offset.x); x < Mathf.Min(wt1.size, wt1.size+offset.x); x++)
        {
            for (int y = Mathf.Max(0, offset.y); y < Mathf.Min(wt1.size, wt1.size + offset.y); y++)
            {
                if (wt1.tileContent[x,y] != wt2.tileContent[x-offset.x, y - offset.y]) { return false; }
            }
        }
        wt1.Compatible[offset].Add(wt2);
        wt2.Compatible[offset*-1].Add(wt1);
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

    

    [SerializeField, Header("Generation Options")]
    private int PatternSize = 3;
    [SerializeField]
    private int Precision = 5;
    [SerializeField]
    private bool XClamp = false, YClamp = false;
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
        currentGrid.isXClamped = XClamp;
        currentGrid.isYClamped = YClamp;
        currentGrid.TILESIZE = PatternSize;
        currentGrid.setPrecision(Precision);
        inputGrid = GOInput.GetComponent<SpriteCreator>().grid;
        inputGrid.isXClamped = XClamp;
        inputGrid.isYClamped = YClamp;

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
