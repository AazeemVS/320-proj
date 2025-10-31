using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
    GameObject bg1, bg2;
    [SerializeField] float bgScrollSpeed = 2;
    [SerializeField] GameObject UI;
    float offScreenX;
    float bgSpriteWidth;
    float bg1LastX, bg2LastX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Camera cam = Camera.main;
        float borderY = cam.orthographicSize;
        float borderX = borderY * cam.aspect;
        //define background values based off of screen size
        bg1 = GameObject.FindGameObjectsWithTag("Background")[0];
        bg2 = GameObject.FindGameObjectsWithTag("Background")[1];
        if (bg1 == null || bg2 == null) throw new UnassignedReferenceException("Could not find background objects. Make sure they have the background tag");
        bgSpriteWidth = bg1.GetComponent<SpriteRenderer>().bounds.size.x/2;
        offScreenX = -bgSpriteWidth + borderX;
        //define UI values based off of screen size
        SpriteRenderer TopUI = UI.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer BottomUI = UI.transform.GetChild(1).GetComponent<SpriteRenderer>();
        float UIWidth = TopUI.bounds.size.x;
        float UIScaling = 2*borderX/UIWidth;
        TopUI.transform.localScale *= UIScaling;
        BottomUI.transform.localScale *= UIScaling;
        TopUI.transform.position = new Vector3(0, borderY - TopUI.bounds.size.y / 2, -9);
        BottomUI.transform.position = new Vector3(0, -borderY + BottomUI.bounds.size.y / 2, -9);

    }

    // Update is called once per frame
    void Update()
    {
        UpdateBackground();
    }


    //move and loop background
    void UpdateBackground() {
        bg1.transform.position = new Vector3(bg1.transform.position.x - bgScrollSpeed * Time.deltaTime, 0, 10);
        bg2.transform.position = new Vector3(bg2.transform.position.x - bgScrollSpeed * Time.deltaTime, 0, 10);
        if (bg1.transform.position.x <= offScreenX && bg1LastX > offScreenX) {
            Debug.Log("looped");
            bg2.transform.position = new Vector3(bg1.transform.position.x + 2 * bgSpriteWidth - .1f, 0, 10);
        } else if (bg2.transform.position.x <= offScreenX && bg2LastX > offScreenX) {
            Debug.Log("looped");
            bg1.transform.position = new Vector3(bg2.transform.position.x + 2 * bgSpriteWidth - .1f, 0, 10);
        }
        bg1LastX = bg1.transform.position.x;
        bg2LastX = bg2.transform.position.x;
    }
}
