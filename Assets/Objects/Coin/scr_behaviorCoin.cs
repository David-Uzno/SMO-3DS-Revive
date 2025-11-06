using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CoinState { Idle, Collected }

public class scr_behaviorCoin : MonoBehaviour
{
	public CoinState CurrentState = CoinState.Idle;
	private Animator _coinAnimator;
	private GameObject _coinChildMesh;

	private void Start()
	{
		_coinChildMesh = transform.GetChild(0).gameObject;
		_coinAnimator = _coinChildMesh.GetComponent<Animator>();

		if (CurrentState == CoinState.Collected)
		{
			CollectCoin();
		}
		else
		{
			this.enabled = false;
		}
	}

	private void Update()
	{
		CheckAnimationEnd();
	}

	private void CheckAnimationEnd()
	{
		if (_coinAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
		{
			Destroy(gameObject);
		}
	}

	private void OnTouch(int coinTouchType)
	{
		if (CurrentState == CoinState.Idle && (coinTouchType == 1 || coinTouchType == 2))
		{
			CollectCoin();
		}
	}

	private void CollectCoin()
	{
		HandleCoinCollectEffects();
		HandleCoinCollectAnimation();
		CurrentState = CoinState.Collected;
	}

	private void HandleCoinCollectEffects()
	{
		scr_main.s.coinsCount++; // Añadir moneda al conteo global
		scr_manAudio.s.PlaySND(eSnd.CoinCollect);
        scr_manageEffect.s.Play("prt_coinSpark0", transform.position, transform.rotation, "prt_coinSpark1");
	}

	private void HandleCoinCollectAnimation()
	{
        transform.GetComponent<Collider>().enabled = false;
        _coinAnimator.Play("collect"); // Animación incompleta
	}
}
