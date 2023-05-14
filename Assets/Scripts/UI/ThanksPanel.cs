using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThanksPanel : MonoBehaviour
{
    public void Thanks()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
