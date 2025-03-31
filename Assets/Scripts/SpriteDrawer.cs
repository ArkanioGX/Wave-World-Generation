using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteCreator))]
public class SpriteDrawer : MonoBehaviour
{
    private Camera cameraMain;
    public bool isActive = true;
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
        Ray ray = cameraMain.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (isActive)
        {
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
                if (Input.GetMouseButton(2))
                {
                    sc.grid.SetPixelAt(spritePos.x, spritePos.y, 2);
                }
                sc.ApplyChanges();
            }
        }
    }
}
