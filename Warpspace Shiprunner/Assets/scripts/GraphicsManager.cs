using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class GraphicsManager : MonoBehaviour
{
    [SerializeField] float bgScrollSpeed = 2;
    [SerializeField] GameObject UI;
    [SerializeField] List<GameObject> backgroundSprites;
    [SerializeField] GameObject healthBar;
    float[] bgLastX = new float[6];
    float offScreenX;
    float bgSpriteWidth;
    float bg1LastX, bg2LastX;
    public float playerHealthMax;
    public float playerHealthCurrent;
    float healthbarMaxWidth;
    float healthBarPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Camera cam = Camera.main;
        float borderY = cam.orthographicSize;
        float borderX = borderY * cam.aspect;
        //define UI values based off of screen size
        SpriteRenderer TopUI = UI.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer BottomUI = UI.transform.GetChild(1).GetComponent<SpriteRenderer>();
        float UIWidth = TopUI.bounds.size.x;
        float UIScaling = 2*borderX/UIWidth;
        TopUI.transform.localScale *= UIScaling;
        BottomUI.transform.localScale *= UIScaling;
        TopUI.transform.position = new Vector3(0, borderY - TopUI.bounds.size.y / 2, -9);
        BottomUI.transform.position = new Vector3(0, -borderY + BottomUI.bounds.size.y / 2, -9);
        healthbarMaxWidth = healthBar.transform.localScale.x;
        healthBarPos = healthBar.transform.position.x - healthBar.GetComponent<SpriteRenderer>().bounds.size.x / 2;
        player_movement player = GameObject.FindGameObjectWithTag("Player").GetComponent<player_movement>();
        player.topUIHeight = TopUI.bounds.size.y;
        player.bottomUIHeight = BottomUI.bounds.size.y;

    }

    // Update is called once per frame
    void Update()
    {
        UpdateBackground(0, 1.5f);
        UpdateBackground(2, 2.5f);
        UpdateBackground(4, 5.5f);
    }

    public void UpdateHealthbar(float newHealth){
        playerHealthCurrent = newHealth;
        healthBar.transform.localScale = new Vector3(healthbarMaxWidth*playerHealthCurrent/playerHealthMax, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
        float newWidth = healthBar.GetComponent<SpriteRenderer>().bounds.size.x/2;
        //left align healthbar to healthbar box
        healthBar.transform.position = new Vector3(healthBarPos + newWidth, healthBar.transform.position.y, healthBar.transform.position.z);
    }

    //move and loop background
    void UpdateBackground(int layer, float scrollSpeed) {
        bgSpriteWidth = backgroundSprites[layer].GetComponent<SpriteRenderer>().bounds.size.x / 2;
        Camera cam = Camera.main;
        float borderY = cam.orthographicSize;
        float borderX = borderY * cam.aspect;
        offScreenX = -bgSpriteWidth + borderX;
        backgroundSprites[layer].transform.position = new Vector3(backgroundSprites[layer].transform.position.x - scrollSpeed * Time.deltaTime, 0, -layer + 5);
        backgroundSprites[layer + 1].transform.position = new Vector3(backgroundSprites[layer + 1].transform.position.x - scrollSpeed * Time.deltaTime, 0, -layer + 5);
        if (backgroundSprites[layer].transform.position.x <= offScreenX && bgLastX[layer] > offScreenX) {
            backgroundSprites[layer + 1].transform.position = new Vector3(backgroundSprites[layer].transform.position.x + 2 * bgSpriteWidth - .1f, 0, 10);
        } else if (backgroundSprites[layer + 1].transform.position.x <= offScreenX && bgLastX[layer + 1] > offScreenX) {
            backgroundSprites[layer].transform.position = new Vector3(backgroundSprites[layer + 1].transform.position.x + 2 * bgSpriteWidth - .1f, 0, 10);
        }
        bgLastX[layer] = backgroundSprites[layer].transform.position.x;
        bgLastX[layer + 1] = backgroundSprites[layer + 1].transform.position.x;
    }
}
