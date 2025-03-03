using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(SpriteCreator))]
public class SpriteDrawer : MonoBehaviour
{
    private Camera cameraMain;
    // Start is called before the first frame update
    void Start()
    {
        cameraMain = Camera.main;
        //GetComponent<SpriteCreator>().Fill(color2);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (cameraMain == null)
        {
            cameraMain = Camera.main; 
        }
        Ray ray = cameraMain.ScreenPointToRay(Input.mousePosition);

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
                sc.grid.SetPixelAt(spritePos.x, spritePos.y, 1);
            }
            if (Input.GetMouseButton(1))
            {
                sc.grid.SetPixelAt(spritePos.x, spritePos.y, 0);
            }
            if (Input.GetMouseButtonDown(2))
            {
                sc.grid.Fill(0);
            }
            sc.ApplyChanges();
        }
    }
}
