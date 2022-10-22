using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mode : MonoBehaviour
{
    private Animator _anime;
    private bool _mode;
    void Start()
    {
        _anime = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && !_mode)
        {
            _anime.SetTrigger("Slash");
        }
    }
}
