using UnityEngine;


public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] SpriteRenderer topThruster, mainThruster, bottomThruster;
    Vector2 sideThrusterScaling, mainThrusterScaling;
    private int direction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sideThrusterScaling = topThruster.transform.localScale;
        mainThrusterScaling = mainThruster.transform.localScale;
    }

    public void HandleMovementAnimation(Vector2 movement, bool dashing = false) {
        int tempDir = 0;
        if (movement.x > 0) tempDir += 1; else if (movement.x < 0) tempDir += 2;
        if (movement.y > 0) tempDir += 10; else if (movement.y < 0) tempDir += 20;
        if (tempDir != direction) AnimateThruster(tempDir);
        direction = tempDir;
    }

    private void AnimateThruster(int dir) {
        switch (dir) {
            case 00:
                mainThruster.enabled = true;
                topThruster.enabled = false;
                bottomThruster.enabled = false;
                mainThruster.transform.localScale = mainThrusterScaling;

                break;
            case 01:
                mainThruster.enabled = true;
                topThruster.enabled = true;
                bottomThruster.enabled = true;
                mainThruster.transform.localScale = new Vector2(mainThrusterScaling.x, mainThrusterScaling.y * 1.5f);
                break;
            case 02:
                mainThruster.enabled = false;
                topThruster.enabled = false;
                bottomThruster.enabled = false;
                break;
            case 10:
                mainThruster.enabled = true;
                topThruster.enabled = false;
                bottomThruster.enabled = true;
                mainThruster.transform.localScale = mainThrusterScaling;
                break;
            case 11:
                mainThruster.enabled = true;
                topThruster.enabled = false;
                bottomThruster.enabled = true;
                mainThruster.transform.localScale = new Vector2(mainThrusterScaling.x, mainThrusterScaling.y * 1.5f);
                break;
            case 12:
                mainThruster.enabled = false;
                topThruster.enabled = false;
                bottomThruster.enabled = true;
                break;
            case 20:
                mainThruster.enabled = true;
                topThruster.enabled = true;
                bottomThruster.enabled = false;
                mainThruster.transform.localScale = mainThrusterScaling;
                break;
            case 21:
                mainThruster.enabled = true;
                topThruster.enabled = true;
                bottomThruster.enabled = false;
                mainThruster.transform.localScale = new Vector2(mainThrusterScaling.x, mainThrusterScaling.y * 1.5f);
                break;
            case 22:
                mainThruster.enabled = false;
                topThruster.enabled = true;
                bottomThruster.enabled = false;
                break;

        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
