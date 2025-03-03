using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SpriteDrawer : MonoBehaviour
{
    Camera camera;
    public Color color1;
    public Color color2;
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        //GetComponent<SpriteCreator>().Fill(color2);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (camera == null)
        {
            camera = Camera.main; 
        }
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject != gameObject)
            {
                return;
            }
                

            SpriteCreator sc = hit.collider.gameObject.GetComponent<SpriteCreator>();
            Vector2Int spritePos = sc.WorldToSpritePos(hit.point);
            if (Input.GetMouseButton(0))
            {
                sc.SetPixel(spritePos.x, spritePos.y, color1);
                sc.Apply();
            }
            if (Input.GetMouseButton(1))
            {
                sc.SetPixel(spritePos.x, spritePos.y, color2);
                sc.Apply();
            }
            if (Input.GetMouseButtonDown(2))
            {
                sc.Fill(color2);
                sc.Apply();
            }
        }
    }
}
