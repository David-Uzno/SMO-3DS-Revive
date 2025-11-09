using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CoinState { Idle, Collected }

public class scr_behaviorCoin : MonoBehaviour
{
	public CoinState CurrentState = CoinState.Idle;
	[SerializeField] private bool _useCollectAnimation = false;
	private Animator _coinAnimator;
	private GameObject _coinChildMesh;

	private void Start()
	{
		_coinChildMesh = transform.GetChild(0).gameObject;
		_coinAnimator = _coinChildMesh.GetComponent<Animator>();

		if (CurrentState == CoinState.Collected)
		{
			this.enabled = true;
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
		if (_coinAnimator == null) return;

		var state = _coinAnimator.GetCurrentAnimatorStateInfo(0);

		if (state.IsName("collect") && state.normalizedTime > 1f)
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
		CurrentState = CoinState.Collected;
		HandleCoinCollectEffects();
		if (_useCollectAnimation)
		{
			HandleCoinCollectAnimation();
		}
		else
		{
			DestroyCoinImmediate();
		}
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
		_coinAnimator.Play("collect");
		this.enabled = true;
	}

	private void DestroyCoinImmediate()
	{
		Destroy(gameObject);
	}
}
