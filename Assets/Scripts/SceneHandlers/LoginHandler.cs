using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Proyecto26;

public class LoginHandler : MonoBehaviour
{
    private Dictionary<string, string> paramz;
    //网络请求
    private const string PC_REGISTER = HttpsUtils.DNS_URL + "/pokemon/register";
    private const string PC_LOGIN = HttpsUtils.DNS_URL + "/pokemon/login";
    private bool running;
    private Button loginBtn, registerBtn;
    private RawImage back;
    private InputField userNameInputField;
    private InputField qqInputField;


    private void Awake()
    {
        back = transform.GetComponent<RawImage>();
        loginBtn = transform.Find("LoginButton/LoginQQIn").GetComponent<Button>();
        registerBtn = transform.Find("LoginButton/LoginRegister").GetComponent<Button>();
        userNameInputField = transform.Find("LoginUserEdit/Input").GetComponent<InputField>();
        qqInputField = transform.Find("LoginQQEdit/Input").GetComponent<InputField>();
    }

    void Start()
    {
        loginBtn.onClick.AddListener(LoginClick);
        registerBtn.onClick.AddListener(LoginClick);
        Scene.main.SetMapButtonVisible(false);
        StartCoroutine("animateBack");
    }

    public IEnumerator control()
    {
        Scene.main.SetMapButtonVisible(false);
        yield return StartCoroutine("animateBack");
        running = true;
        while (running)
        {
            yield return null;
        }
        StopCoroutine("animateBack");
        Scene.main.SetMapButtonVisible(true);
        this.gameObject.SetActive(false);
    }

    public void AndroidCallBack(string openid)
    {
        Debug.Log("AndroidCallBack");
        paramz = new Dictionary<string, string>();
        HttpsUtils.SetGeneralParams(paramz);
        RequestHelper request = new RequestHelper
        {
            Uri = PC_REGISTER,
            Params = paramz,
            Method = "POST",
            Body = new LoginRegisterReq()
            {
                openId = openid,
                userName = userNameInputField.text,
                qq = qqInputField.text
            }
        };
        RestClient.Request(request).Then(loginDatas =>
        {
            LoginRegisterResult result = JsonUtility.FromJson<LoginRegisterResult>(loginDatas.Text);
            //缓存数据
            if (result == null || result.code != 200)
            {
                StartCoroutine(destoryApplication(result.msg));
                return;
            }
            HttpsUtils.SetToken(result.token);
            TrainerManager.Instance.updateData(delegate (bool result)
            {
                if (result)
                {
                    PCManager.Instance.fetchPartyPokemonList(delegate (bool result) {
                        GlobalVariables.global.setRealStart();
                        GlobalVariables.global.checkTaskCompletedObejct();
                        GlobalVariables.global.resetFollower();
                        running = false;
                        StopCoroutine("animateBack");
                        Scene.main.SetMapButtonVisible(true);
                        this.gameObject.SetActive(false);
                    });
                }
                else
                {
                    StartCoroutine(CommonUtils.showDialogText("拉取用户信息失败"));
                }
            });
        }).Catch(err =>
        {
            Debug.Log("register请求失败:" + err);
            StartCoroutine(CommonUtils.showDialogText("登录失败，你可能不是目标玩家"));
            CommonUtils.onDestoryApplication();
        });
    }

    public IEnumerator destoryApplication(string msg)
    {
        yield return StartCoroutine(CommonUtils.showDialogText(msg));
        CommonUtils.onDestoryApplication();
    }

    public void LoginClick()
    {
        AndroidCallBack("895766086");
        //Debug.Log("LoginClick");
        //AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        //AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        //jo.Call("LoginQQ");
    }

    private IEnumerator animateBack()
    {
        List<Texture> animation = CommonUtils.loadTexturesFromGif("back.gif");
        yield return CommonUtils.animateImage(animation, back);
    }
}
