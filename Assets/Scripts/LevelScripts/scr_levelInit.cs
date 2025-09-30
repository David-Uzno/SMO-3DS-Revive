using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_levelInit : MonoBehaviour {


	public string namee = "Hat";

	void Start () {
		//WIP, just adding bgmplay for demo.
		scr_manAudio.s.PlayBGM(namee);
	}
}
