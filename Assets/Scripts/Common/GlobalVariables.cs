using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalVariables : MonoBehaviour
{
    public AudioClip ballPlaceClip;
    public AudioClip healMFX;

    public int callbackCalls;
    public UnityEngine.Events.UnityEvent onConnect;
    public UnityEngine.Events.UnityEvent onDisconnect;

    public static GlobalVariables global;
    public bool playerForwardOnLoad;

    public bool fadeIn;
    public Texture fadeTex;

    public int followerIndex = 0;
    //private FollowerMovement FollowerSettings;

    public AudioClip surfBGM;
    public int surfBgmLoopStart;
    public bool respawning = false;
    private PlayerConfig playerConfig;
    private int playerConfigRecordNo;
    private long[] registeredItems = new long[4];

    private RawImage[] registeredRawImages = new RawImage[4];
    private Text[] registeredTexts = new Text[4];
    private Button[] registeredButtons = new Button[4];
    //标记游戏主题是否真的开始
    private bool realStart = false;
    //任务完成可以隐藏的GameObject
    public string[] taskCompletedNames;
    public GameObject[] taskCompletedObjects;

    void Awake()
    {
        callbackCalls = 0;
        SceneManager.sceneLoaded += CheckLevelLoaded;
        if (global == null)
        {
            global = this;
            Object.DontDestroyOnLoad(this.gameObject);
            GL.Clear(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));
            for (int i = 0; i < 4; i++)
            {
                registeredButtons[i] = GameObject.Find("TCKCanvas/RegisterBox/RegiesterButton" + i).GetComponent<Button>();
                registeredRawImages[i] = GameObject.Find("TCKCanvas/RegisterBox/RegiesterButton" + i + "/RawImage").GetComponent<RawImage>();
                registeredTexts[i] = GameObject.Find("TCKCanvas/RegisterBox/RegiesterButton" + i + "/Text").GetComponent<Text>();
            }

            playerConfig = GlobalPreference.Instance.getPrefPlayerConfig();
            if (playerConfig != null)
            {
                if (!string.IsNullOrEmpty(playerConfig.getRespawnSceneName()))
                {
                    SceneManager.LoadScene(playerConfig.getRespawnSceneName());
                }
                if (playerConfig.getRegisteredItems() != null)
                {
                    long[] registeredItems = playerConfig.getRegisteredItems();
                    for (int i = 0; i < registeredItems.Length; i++)
                    {
                        if (registeredItems[i] > 0)
                        {
                            setRegisterItem(i, registeredItems[i]);
                        }
                    }
                }
            }
        }
        else if (global != this)
        {
            Destroy(gameObject);
        }
    }


    void OnDestroy()
    {
        SceneManager.sceneLoaded -= CheckLevelLoaded;
    }

    void CheckLevelLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        //if (SceneManager.GetActiveScene().name != "startup")
        //{
        //    playerConfig = GlobalPreference.Instance.getPrefPlayerConfig();
        //    if (playerConfig != null && !string.IsNullOrEmpty(playerConfig.getRespawnSceneName()))
        //    {
        //        PlayerMovement.player.transform.position = playerConfig.getRespawnScenePosition();
        //        PlayerMovement.player.updateDirection(playerConfig.getPlayerDirection());
        //        PlayerMovement.player.resetMainCameraDiff();
        //    }
        //    if (global == this)
        //    {
        //        if (global.fadeIn)
        //        {
        //            StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.slowedSpeed));
        //            if (playerForwardOnLoad)
        //            {
        //                PlayerMovement.player.forceMoveForward();
        //                playerForwardOnLoad = false;
        //            }
        //        }
        //        else
        //        {
        //            ScreenFade.main.SetToFadedIn();
        //        }
        //    }
        //}
    }

    //注册物品
    public void setRegisterItem(int index, long goodId)
    {
        registeredItems[index] = goodId;
        playerConfig = GlobalPreference.Instance.getPrefPlayerConfig();
        playerConfig.setRegisteredItems(registeredItems);
        GlobalPreference.Instance.setPrefPlayerConfig(playerConfig);
        registeredButtons[index].gameObject.SetActive(true);
        registeredButtons[index].onClick.AddListener(delegate
        {
            if (CommonUtils.isQuickClick())
            {
                return;
            }
            //PlayerMovement.player.interact(goodId);
        });
        ItemData itemData = ItemDatabase.getItem(goodId);
        registeredRawImages[index].texture = Resources.Load<Texture>("Items/" + CommonUtils.convertLongID(goodId));
        registeredTexts[index].text = itemData.getName();
    }

    //保存玩家信息
    public PlayerConfig savePlayerConfig()
    {
        //if (playerConfigRecordNo > 30)
        //{
        //    playerConfig = GlobalPreference.Instance.getPrefPlayerConfig();
        //    playerConfig.setPlayerDirection(PlayerMovement.player.direction);
        //    playerConfig.setRespawnScenePosition(PlayerMovement.player.transform.position);
        //    GlobalPreference.Instance.setPrefPlayerConfig(playerConfig);
        //    playerConfigRecordNo = 0;
        //}
        //playerConfigRecordNo++;
        return playerConfig;
    }

    public bool isRealStart()
    {
        return realStart;
    }

    public void setRealStart()
    {
        realStart = true;
    }

    //检测剧情要隐藏的gameObject
    public void checkTaskCompletedObejct()
    {
        if (taskCompletedNames != null)
        {
            for (int i = 0; i < taskCompletedNames.Length; i++)
            {
                if (TrainerManager.Instance.getTaskValue(taskCompletedNames[i]) > 0)
                {
                    taskCompletedObjects[i].SetActive(false);
                }
            }
        }
    }

    public void resetFollower()
    {
        //if (FollowerSettings == null)
        //{
        //    FollowerSettings = GameObject.Find("Player").GetComponentInChildren<FollowerMovement>();
        //}
        //for (int i = 0; i < 6; i++)
        //{
        //    if (PCManager.Instance.getPartyBox()[i] != null)
        //    {
        //        if (PCManager.Instance.getPartyBox()[i].getStatus() != Pokemon.Status.FAINTED)
        //        {
        //            FollowerSettings.changeFollower(i);
        //            i = 6;
        //        }
        //    }
        //}
    }

    public AudioClip getSurfBGM()
    {
        return surfBGM;
    }

    public void ReadyCallback()
    {
        ++callbackCalls;
        onConnect.Invoke();
    }
    public void DisconnectedCallback(int errorCode, string message)
    {
        ++callbackCalls;
        onDisconnect.Invoke();
    }

    public void ErrorCallback(int errorCode, string message)
    {
        ++callbackCalls;
    }

    //显示全局dialog
    public void ShowDialog(string txtTitle, string txtMessage,
        DialogOnButtonCallback callback)
    {
        //chibiDialog.gameObject.SetActive(true);
        //var cancel = new Chibi.Free.Dialog.ActionButton("取消", () =>
        //{
        //    callback(false, true);
        //    chibiDialog.gameObject.SetActive(false);
        //},
        //    new Color(1f, 0.9f, 0.8f));
        //var ok = new Chibi.Free.Dialog.ActionButton("确定", () =>
        //{
        //    callback(true, false);
        //    chibiDialog.gameObject.SetActive(false);
        //}, new Color(1f, 0.9f, 0.8f));
        //Chibi.Free.Dialog.ActionButton[] buttons = { cancel, ok };
        //chibiDialog.ShowDialog(txtTitle, txtMessage, buttons);
    }

    public delegate void DialogOnButtonCallback(bool abutton, bool bbutton);

}