using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IllustButton : MonoBehaviour
{
    private RectTransform _thisTransform;
    private Vector2 _originPos;
    [SerializeField]
    private short _stage;
    private bool _bSuccess = true;

    private void Awake()
    {
        _thisTransform = GetComponent<RectTransform>();
        _originPos = _thisTransform.anchoredPosition;
        if (GameManager.Instance.Stage.stage <= _stage)
        {
            GetComponent<Image>().color = Color.black;
            _bSuccess = false;
        }
    }

    public void ClickIllust()
    {
        if (_bSuccess)
        {
            if (_thisTransform.sizeDelta.x != 800)
            {
                _thisTransform.sizeDelta = new Vector2(800, 450);
                _thisTransform.SetAsLastSibling();
                _thisTransform.anchoredPosition = Vector2.zero;
            }

            else
            {
                _thisTransform.sizeDelta = new Vector2(192, 108);
                _thisTransform.anchoredPosition = _originPos;
            }
        }
    }
}
