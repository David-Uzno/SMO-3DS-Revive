using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_spn_coin_single : MonoBehaviour {

	public CoinState CurrentState = CoinState.Idle;
	// Use this for initialization
	void Start () {
		scr_summon.s.s_object(0, transform.position, transform.eulerAngles).GetComponent<scr_behaviorCoin>().CurrentState = CurrentState;
		Destroy(gameObject);
	}
}
