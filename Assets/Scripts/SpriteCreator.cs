using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



[AddComponentMenu("SpriteDisplayer")]
public class SpriteCreator : MonoBehaviour
{
    [HideInInspector]
    public WaveFunctionGrid2D grid;
    private SpriteRenderer spriteRenderer;
    private BoxCollider boxCollider;
    [SerializeField]
    private Color[] ColorToUse;
    public InputActionAsset actions;

    [Serializable]
    public struct SpriteInfos
    {
        public Sprite sprite;
        public bool xClamp;
        public bool yClamp;
    }

    [SerializeField]
    private SpriteInfos[] SpriteAvailables;
    private int spriteID = 0;

    [SerializeField]
    private float sizeModifier;
    [SerializeField]
    private Vector2Int textureSize = new Vector2Int(32, 32);
    // Start is called before the first frame update
    private void Awake()
    {
        
       
    }

    void Start()
    {
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        if (SpriteAvailables.Length > 0)
        {
            grid = SpriteToGrid(SpriteAvailables[0].sprite);
            grid.isXClamped = SpriteAvailables[0].xClamp;
            grid.isYClamped = SpriteAvailables[0].yClamp;
            textureSize = grid.size;
            GetComponent<SpriteDrawer>().isActive = false;
        }
        else if (grid == null)
        {
            createGrid();
            spriteRenderer.sprite = grid.GetSprite();
        }
        
        ApplyChanges();
        if (boxCollider)
        {
            boxCollider.size = new Vector3(textureSize.x / 100.0f, textureSize.y / 100.0f, 0.01f);
        }
       
        float maxTextureSize = Mathf.Max(textureSize.x, textureSize.y);
        transform.localScale = Vector3.one*(sizeModifier*(128.0f/maxTextureSize));

        if (actions != null)
        {
            actions.Enable();
            actions.FindActionMap("Generation").FindAction("SwapImage").performed += OnSwap;
        }
    }

    private WaveFunctionGrid2D SpriteToGrid(Sprite s)
    {
        return new WaveFunctionGrid2D(s);
    }

    public WaveFunctionGrid2D createGrid()
    {
        grid = new WaveFunctionGrid2D(textureSize, getColors());
        return grid;
    }

    public WaveFunctionGrid2D getGrid()
    {
        if (grid == null)
        {
            return createGrid();
        }
        return grid;
    }

    public Color[] getColors()
    {
        if (ColorToUse.Length < 2)
        {
            string warningstr = "Not enough color set in " + gameObject.name;
            Debug.LogWarning(warningstr);
            ColorToUse = new Color[2];
            ColorToUse[0] = Color.magenta;
            ColorToUse[1] = Color.blue;
        }
        return ColorToUse;
    }

    public void ApplyChanges()
    {
       if (grid.updateSprite())
            spriteRenderer.sprite = grid.GetSprite();

    }

    /// <summary>
    /// Compute The world pos to the sprite position
    /// </summary>
    /// <param name="pos">World Position to convert</param>
    /// <returns>Sprite Position</returns>
    public Vector2Int WorldToSpritePos(Vector3 pos)
    {
        Vector3 ITPos = transform.InverseTransformPoint(pos);
        Vector2Int gridPos = new Vector2Int(
            (int)((grid.size.x/2.0f) + (ITPos.x*100.0f)),
            (int)((grid.size.y / 2.0f) + (ITPos.y * 100.0f)));
        gridPos = new Vector2Int(
            Mathf.Clamp(gridPos.x, 0, grid.size.x),
            Mathf.Clamp(gridPos.y, 0, grid.size.y));
        return gridPos;
    }

    private void OnSwap(InputAction.CallbackContext context)
    {
        if (SpriteAvailables.Length > 0)
        {
            spriteID = (spriteID + (int)context.ReadValue<float>() + SpriteAvailables.Length) % SpriteAvailables.Length;
            grid = SpriteToGrid(SpriteAvailables[spriteID].sprite);
            grid.isXClamped = SpriteAvailables[spriteID].xClamp;
            grid.isYClamped = SpriteAvailables[spriteID].yClamp;
            textureSize = grid.size;
            GetComponent<SpriteDrawer>().isActive = false;
        }
        else if (grid == null)
        {
            createGrid();
            spriteRenderer.sprite = grid.GetSprite();
        }

        ApplyChanges();
        if (boxCollider)
        {
            boxCollider.size = new Vector3(textureSize.x / 100.0f, textureSize.y / 100.0f, 0.01f);
        }

        float maxTextureSize = Mathf.Max(textureSize.x, textureSize.y);
        transform.localScale = Vector3.one * (sizeModifier * (128.0f / maxTextureSize));
    }
}
