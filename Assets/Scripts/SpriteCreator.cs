using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SpriteDisplayer")]
public class SpriteCreator : MonoBehaviour
{
    private Texture2D displayTexture;
    private Sprite displaySprite;
    private SpriteRenderer spriteRenderer;
    private BoxCollider boxCollider;
    [SerializeField]
    private Color baseColor;
    [SerializeField]
    private float sizeModifier;
    [SerializeField]
    private Vector2Int textureSize = new Vector2Int(32, 32);
    [SerializeField]
    private FilterMode currentFilter = FilterMode.Point;
    [SerializeField]
    private GameObject sphereDebug;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider>();
       // boxCollider.size = new Vector3(textureSize.x, textureSize.y, 0.01f);
        transform.localScale = Vector3.one*(sizeModifier*(128.0f/textureSize.x));
        displayTexture = new Texture2D(textureSize.x,textureSize.y);
        displayTexture.filterMode = currentFilter;
        displayTexture.wrapMode = TextureWrapMode.Clamp;
        Fill(baseColor);
        Apply();
    }

    public void SetPixel(int x, int y, Color newColor)
    {
        displayTexture.SetPixel(x, y, newColor);
    }
    public void Fill(Color newColor)
    {
        for (int x = 0; x < displayTexture.width; x++) {
            for (int y = 0; y < displayTexture.height; y++) {
                SetPixel(x,y, newColor);
            }
        }
    }

    public void Apply()
    {
        displayTexture.Apply();
        displaySprite = Sprite.Create(displayTexture, new Rect(0, 0, displayTexture.width, displayTexture.height), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = displaySprite;
    }

    /// <summary>
    /// Compute The world pos to the sprite position
    /// </summary>
    /// <param name="pos">World Position to convert</param>
    /// <returns>Sprite Position</returns>
    public Vector2Int WorldToSpritePos(Vector3 pos)
    {
        Vector3 ITPos = transform.InverseTransformPoint(pos);
        Debug.Log(displayTexture.width / 2);
        Vector2Int gridPos = new Vector2Int(
            (int)((displayTexture.width/2.0f) + (ITPos.x*100.0f)),
            (int)((displayTexture.height / 2.0f) + (ITPos.y * 100.0f)));
        gridPos = new Vector2Int(
            Mathf.Clamp(gridPos.x, 0, displayTexture.width),
            Mathf.Clamp(gridPos.y, 0, displayTexture.height));
        Debug.Log(gridPos);
        return gridPos;
    }
}
