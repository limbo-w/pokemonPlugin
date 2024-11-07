using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class UScrollRect : ScrollRect
{
    public RectTransform rect;
    /// 高度 往下拉是负数   往上拉是正数
    float dropValue = -30f;
    /// 是否刷新
    bool isDownRefresh = false;
    bool isUpRefresh = false;
    /// 是否处于拖动
    bool isDrag = false;
    /// 显示、隐藏刷新字段
    public Action<bool> OnValue;
    /// 如果满足刷新条件 执行的方法
    public Action OnDownRefresh;
    public Action OnUpRefresh;

    protected override void Awake()
    {
        base.Awake();
        rect = GetComponentInChildren<ContentSizeFitter>().GetComponent<RectTransform>();
        onValueChanged.AddListener(ScrollValueChanged);
    }

    /// 当ScrollRect被拖动时
    /// <param name="vector">被拖动的距离与Content的大小比例</param>
    void ScrollValueChanged(Vector2 vector)
    {
        //如果不拖动 当然不执行之下的代码
        if (!isDrag)
        {
            return;
        }
        //如果拖动的距离大于给定的值
        if (rect.anchoredPosition.y < dropValue)
        {
            isDownRefresh = true;
            if (OnValue != null)
            {
                OnValue(isDownRefresh);
            }
        }
        else if (rect.anchoredPosition.y > -dropValue)
        {
            isUpRefresh = true;
            if (OnValue != null)
            {
                OnValue(isUpRefresh);
            }
        }
        else
        {
            isUpRefresh = false;
            isDownRefresh = false;
            if (OnValue != null)
            {
                OnValue(false);
            }
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        isDrag = true;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (OnValue != null)
        {
            OnValue(false);
        }
        if (isDownRefresh)
        {
            if (OnDownRefresh != null)
            {
                OnDownRefresh();
            }
        }
        if (isUpRefresh)
        {
            if (OnUpRefresh != null)
            {
                OnUpRefresh();
            }
        }
        isDownRefresh = false;
        isUpRefresh = false;
        isDrag = false;
    }
}
