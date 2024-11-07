using Proyecto26;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CostumeHandler : MonoBehaviour
{
    private Transform costumePanel;
    private Transform previewPanel, smallPreview;

    private Button finishButton;

    private CostumePreview headPreview;
    private CostumePreview bodyPreview;
    private CostumePreview basePreview;

    private string pngPath = "./Assets/Resources/Costumes/costume_new.png";
    private CostumeList costumeList;

    Dictionary<string, string> headers = new Dictionary<string, string>();
    Dictionary<string, string> paramz = new Dictionary<string, string>();

    private const string FETCH_COSTUMES = HttpsUtils.DNS_URL + "/pokemon/fashion/list";

    void Awake()
    {
        costumePanel = transform.Find("CostumePanel");
        previewPanel = costumePanel.Find("PreviewPanel");
        smallPreview = previewPanel.Find("SmallPreview");

        headPreview = smallPreview.Find("Head").GetComponent<CostumePreview>();
        bodyPreview = smallPreview.Find("Body").GetComponent<CostumePreview>();
        basePreview = smallPreview.Find("Base").GetComponent<CostumePreview>();

        finishButton = costumePanel.Find("FinishButton").GetComponent<Button>();

        finishButton.onClick.AddListener(OnFinish);
        Button returnBtn = transform.Find("Return").GetComponent<Button>();
        returnBtn.onClick.AddListener(delegate
        {
            //todo 退出操作
        });
        OnSceneCostumeLoad();
    }

    void Start()
    {    
        gameObject.SetActive(false);
    }

    void OnFinish()
    {
        Texture2D[] textures = { basePreview.atlas[0].texture, headPreview.atlas[0].texture, bodyPreview.atlas[0].texture };
        ToPng(textures);
    }

    //组合转化成png图片
    void ToPng(Texture2D[] textures)
    {
        Color[] sourceColor;

        int width = textures[0].width, height = textures[0].height;

        Texture2D newT = new Texture2D(width, height, TextureFormat.ARGB32, false);
        int length = width * height;
        Color[] newColor = new Color[length];

        int newIndex, sourceIndex;

        for (int i = 0; i < textures.Length; i++)
        {
            sourceColor = textures[i].GetPixels();
            newIndex = 0;
            for (int y = 0; y < height; y++)
            {
                sourceIndex = y * width;
                for (int x = 0; x < width; x++)
                {
                    if (sourceColor[sourceIndex].a != 0)
                    {
                        newColor[newIndex++] = sourceColor[sourceIndex++];
                    }
                    else
                    {
                        newIndex++;
                        sourceIndex++;
                    }
                }
            }
        }
        newT.SetPixels(newColor);
        newT.Apply();
        byte[] bytes = newT.EncodeToPNG();
        File.WriteAllBytes(pngPath, bytes);

    }

    void OnSceneCostumeLoad()
    {
        HttpsUtils.SetGeneralHeaders(headers);
        HttpsUtils.SetGeneralParams(paramz);

        RequestHelper request = new RequestHelper
        {
            Uri = FETCH_COSTUMES,
            Headers = headers,
            Params = paramz,
            Method = "POST",
        };

        RestClient.Request(request).Then(result =>
        {
            //string response = result.Text;
            //Debug.Log(response);

            ///
            

            ///


        }).Catch(err =>
        {
            Debug.Log("����ʧ��:" + err);
        });


        string response = "{\"total\":3,\"rows\":[{\"aHfId\":1,\"aRId\": 9,\"aBfIds\": \"1\"},{\"aHfId\":2,\"aRId\": 9,\"aBfIds\": \"1\"}," +
            "{\"aHfId\":3,\"aRId\": 9,\"aBfIds\": \"1\"}],\"code\": 200," +
            "\"msg\": \"����ɹ�\",\"pageNum\": null}";

        costumeList = JsonUtility.FromJson<CostumeList>(response);
    }

    public int[] GetCostumeIDArray()
    {
        if (costumeList == null) return null;
        int[] costumeIDArray = new int[costumeList.rows.Count];

        for (int i = 0; i < costumeList.rows.Count; i++)
        {
            costumeIDArray[i] = costumeList.rows[i].aHfId;
        }
        return costumeIDArray;
    }
}
