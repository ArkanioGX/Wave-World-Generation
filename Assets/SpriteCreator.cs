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
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(textureSize.x, textureSize.y, 0.01f);
        transform.localScale = Vector3.one*(sizeModifier*(128.0f/textureSize.x));
        displayTexture = new Texture2D(textureSize.x,textureSize.y);
        displayTexture.filterMode = currentFilter;
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

    public Vector2Int WorldToSpritePos(Vector3 pos)
    {
        pos = transform.InverseTransformPoint(pos);
        //Debug.Log(pos);
        Vector3 fwdFromCenterPoint =  pos - boxCollider.center;
        fwdFromCenterPoint = transform.rotation * fwdFromCenterPoint;
        fwdFromCenterPoint = new Vector3(fwdFromCenterPoint.x/(boxCollider.size.x),fwdFromCenterPoint .y / (boxCollider.size.y), 0);
        Vector2Int gridPos = new Vector2Int(
            (displayTexture.width/2) + Mathf.RoundToInt(pos.x*100),
            (displayTexture.height / 2) + Mathf.RoundToInt(pos.y *100));
        gridPos = new Vector2Int(
            Mathf.Clamp(gridPos.x, 0, displayTexture.width),
            Mathf.Clamp(gridPos.y, 0, displayTexture.height));
        return gridPos;
    }
}
