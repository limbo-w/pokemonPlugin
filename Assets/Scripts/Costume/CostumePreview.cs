using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class CostumePreview : MonoBehaviour
{

    public Image sprite;
    public Sprite[] atlas;
    float speed = 0.25f;
    float time;
    int index;
    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<Image>();
        index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(time > speed)
        {
            sprite.sprite = atlas[index + 8];
            if (index >= 3) { index = 0; }
            index++;
            time = 0;
        }
        else
        {
            time += Time.deltaTime;
        }
        
    }
}
