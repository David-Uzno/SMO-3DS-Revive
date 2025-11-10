using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_behaviorFrailBox : MonoBehaviour
{
    private Animator frailBoxAnimator;
    private int currentState = 0;
    private int hitCount = 0;
    [SerializeField] private int _hitMax = 14;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        frailBoxAnimator = GetComponent<Animator>();
    }

    public void OnTouch(int touchType)
    {
        if (touchType == 1)
        {
            hitCount++;
            HandleBreak();
        }
    }

    private void HandleBreak()
    {
        hitCount = 0;
        PlayDamageAnimation();

        switch (currentState)
        {
            case 1:
                SetBoxState1();
                break;
            case 2:
                SetBoxState2();
                break;
            case 3:
                DestroyBox();
                break;
        }
        currentState++;
    }

    private void PlayDamageAnimation()
    {
        if (frailBoxAnimator != null)
            frailBoxAnimator.CrossFade("damage", 0.1f);
    }

    private void SetBoxState1()
    {
        Transform boxParts = transform.GetChild(1);
        boxParts.GetChild(0).gameObject.SetActive(false);
        boxParts.GetChild(1).gameObject.SetActive(true);
        boxParts.GetChild(2).gameObject.SetActive(true);
    }

    private void SetBoxState2()
    {
        Transform boxParts = transform.GetChild(1);
        boxParts.GetChild(1).gameObject.SetActive(false);
        hitCount = _hitMax - 2;
    }

    private void DestroyBox()
    {
        Instantiate(Resources.Load<GameObject>("Objects/objFrailBoxTrace"), transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
