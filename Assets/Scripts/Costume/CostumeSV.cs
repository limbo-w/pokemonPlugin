using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CostumeSV : MonoBehaviour
{
    public string part;  
    public CostumeHandler costumeHandler;
    public Transform content;
    RectTransform m_contentRect;
    public GameObject itemPrefab;
    public Button buttonLeft;
    public Button buttonRight;

    public Sprite unselectedBorder, selectedBorder;
    public CostumePreview preview;

    public float itemSpacing = 25f;
    public float itemWidth = 50f;
    public float selectedScale = 1.3f;

    List<Transform> itemsList = new List<Transform>();
    List<Sprite[]> spritesList = new List<Sprite[]>();
    int currentSelectedItemIndex, preSelectedItemIndex;

    // Start is called before the first frame update
    void Start()
    {
        currentSelectedItemIndex = 0;
        preSelectedItemIndex = 0;
        m_contentRect = (RectTransform)content;
        int[] costumeIDs = costumeHandler.GetCostumeIDArray();

        foreach (int id in costumeIDs)
        {
            Sprite[] sprites = loadSpritesByName(part + "_" + id);
            if (sprites != null && sprites.Length > 0)
            {
                spritesList.Add(sprites);
            }
        }

        DrawItems(spritesList.Count);

        UpdatePreviewAtlas();
        UpdatePosition();

        buttonLeft.onClick.AddListener(OnClickPre);
        buttonRight.onClick.AddListener(OnClickNext);
    }

    void DrawItems(int count)
    {
        GameObject obj = Instantiate(itemPrefab);
        obj.transform.SetParent(content);
        DrawItemContent(obj, false, count - 1);
        for (int i = 0; i < count; i++)
        {
            obj = Instantiate(itemPrefab);
            obj.transform.SetParent(content);
            itemsList.Add(obj.transform);

            DrawItemContent(obj, i == currentSelectedItemIndex, i);      
            
        }
        obj = Instantiate(itemPrefab);
        obj.transform.SetParent(content);
        DrawItemContent(obj, false, 0);

    }

    Sprite[] loadSpritesByName(string name)
    {
        return Resources.LoadAll<Sprite>("Costumes/" + part + "/" + name);
    }

    void DrawItemContent(GameObject item, bool selected, int index)
    {
        item.transform.localScale = Vector3.one * (selected ? selectedScale : 1.0f);
        item.transform.GetChild(0).GetComponent<Image>().sprite = selected ? selectedBorder : unselectedBorder;
        item.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = spritesList[index][8];
        item.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = index.ToString();     
    }

    void OnCurrentIndexChanged()
    {
        UpdatePosition();
        UpdateItemBorder();
        UpdatePreviewAtlas();
    }
    void UpdatePosition()
    {
        float contentWidth = (itemsList.Count + 2) * itemWidth + (itemsList.Count + 1) * itemSpacing;
        Vector2 pos = m_contentRect.anchoredPosition;
        pos[0] = contentWidth / 2 - 1.5f * itemWidth - itemSpacing - (currentSelectedItemIndex) * (itemSpacing + itemWidth);
        m_contentRect.anchoredPosition = pos;
    }

    void UpdateItemBorder()
    {
        itemsList[preSelectedItemIndex].GetChild(0).GetComponent<Image>().sprite = unselectedBorder;
        itemsList[preSelectedItemIndex].localScale = Vector3.one;
        itemsList[currentSelectedItemIndex].GetChild(0).GetComponent<Image>().sprite = selectedBorder;
        itemsList[currentSelectedItemIndex].localScale = Vector3.one * selectedScale;
    }

    void UpdatePreviewAtlas()
    {
        preview.atlas = spritesList[currentSelectedItemIndex];
    }
    void OnClickPre()
    {
        preSelectedItemIndex = currentSelectedItemIndex;
        currentSelectedItemIndex -= 1;
        if (currentSelectedItemIndex < 0)
        {
            currentSelectedItemIndex = itemsList.Count - 1;
        }
        OnCurrentIndexChanged();
    }
    void OnClickNext()
    {
        preSelectedItemIndex = currentSelectedItemIndex;
        currentSelectedItemIndex += 1;
        if (currentSelectedItemIndex >= itemsList.Count)
        {
            currentSelectedItemIndex = 0;
        }
        OnCurrentIndexChanged();
    }
}
