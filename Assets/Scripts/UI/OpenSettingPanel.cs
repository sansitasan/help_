using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenSettingPanel : MonoBehaviour
{
    public void OpenOption()
    {
        OptionPanel.instance.gameObject.SetActive(true);
    }
}
