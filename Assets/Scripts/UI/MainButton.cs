using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainButton : MonoBehaviour, IPointerClickHandler
{
    public short level;
    public Action<short> onClick;

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(level);
    }
}
