using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.PlayerSettings;

public class ButtonEvent : MonoBehaviour,ISubmitHandler, ISelectHandler, IDeselectHandler
{
    // Start is called before the first frame update
    void Start()
    {
    }

    public void OnDeselect(BaseEventData eventData)
    {
      
    }

    public void OnSelect(BaseEventData eventData)
    {
       
    }

    public void OnSubmit(BaseEventData eventData)
    {
        
    }
}
