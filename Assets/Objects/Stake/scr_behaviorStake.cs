using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_behaviorStake : MonoBehaviour
{
	[Header("Mounth")]
    private readonly string _cappyMountPath = "Armature/AllRoot/Center/Stake/JointRoot/Bend0/Bend1/Bend2/CapPoint";

	[Header("Bent")]
	private float bentHorizontal;
    private float bentVertical;
    private float bentAmount;
    private readonly float bentAmountMax = 20f;

    [Header("Refences Bones")]
    private Transform boneBend0;
    private Transform boneBend1;
    private Transform boneBend2;
    private Transform cappyBone;

	[Header("Animation")]
	private Animator stakeAnimator;

    private void Start()
    {
		InitializeComponents();
        this.enabled = false;
    }

	private void Update()
	{
		if (Time.timeScale <= 0) return;

		HandleBentAmount();
		ApplyBoneBending();
		AdjustCappyOrientation();
	}

	private void InitializeComponents()
	{
        stakeAnimator = GetComponent<Animator>();
        boneBend0 = transform.Find("Armature/AllRoot/Center/Stake/JointRoot/Bend0");
        boneBend1 = boneBend0.GetChild(0);
        boneBend2 = boneBend1.GetChild(0);
        cappyBone = transform.Find(_cappyMountPath);
	}


	private void HandleBentAmount()
	{
		if (bentAmount < bentAmountMax && bentAmount != -0.1f)
		{
			bentAmount += 0.5f;
		}
		else if (bentAmount != -0.1f)
		{
			// Stake extraído completamente
			GetComponent<Collider>().enabled = false;
			stakeAnimator.Play("pullOut");
			MarioController.s.cappy.SetState(eStateCap.UnHack);
			bentAmount = -0.1f;
		}
		else if (stakeAnimator.GetBool("isDead"))
		{
			Destroy(gameObject);
		}
		else
		{
			// Lógica para "volar"
		}
	}

	private void ApplyBoneBending()
	{
		Vector3 bendEuler = new Vector3(0, bentVertical * bentAmount, bentHorizontal * bentAmount);
		boneBend0.localEulerAngles = bendEuler;
		boneBend1.localEulerAngles = bendEuler;
		boneBend2.localEulerAngles = bendEuler;
	}

	private void AdjustCappyOrientation()
	{
		cappyBone.localRotation = Quaternion.LookRotation(Vector3.up, cappyBone.up);
		cappyBone.localEulerAngles = new Vector3(-cappyBone.localEulerAngles.y, 0, 90);
	}
	
    public void OnCappyTrigger()
    {
        scr_main.s.capMountPoint = _cappyMountPath;
    }

    public void OnCappyHacked()
    {
        MarioController.s.cappy.SetTransformOffset(2, Vector3.zero, Vector3.zero);
        stakeAnimator.Play("pull");
        bentHorizontal = scr_manageInput.AxisDir(-1).x;
        bentVertical = scr_manageInput.AxisDir(-1).y;
        this.enabled = true;
    }
}
