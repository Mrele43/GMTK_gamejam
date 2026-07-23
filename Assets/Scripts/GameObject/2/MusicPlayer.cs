using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour, IInteractable
{
    [Header("TargetID")]
    public string itemID = "MusicPlayer";
    public string itemName = "MusicPlayer";

    public void Interact()
    {

        OnUseEffect();
    }

    public string GetInteractTip()
    {
        return "?E???????";
    }

    private void OnUseEffect()
    {

        TaskManager.Instance.NotifyUseItem(itemID);


        // AudioMgr.Instance.PlaySFX("MusicPlayer");
        
    }
}
