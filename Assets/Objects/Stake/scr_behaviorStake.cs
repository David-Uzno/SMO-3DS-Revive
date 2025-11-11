using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_behaviorStake : MonoBehaviour
{
	[Header("Mounth")]
    private readonly string _cappyMountPath = "Armature/AllRoot/Center/Stake/JointRoot/Bend0/Bend1/Bend2/CapPoint";

	[Header("Bent")]
	private float _bentHorizontal;
    private float _bentVertical;
    private float _bentAmount;
    private readonly float _bentAmountMax = 20f;

    [Header("Refences Bones")]
    private Transform _boneBend0;
    private Transform _boneBend1;
    private Transform _boneBend2;
    private Transform _cappyBone;

	[Header("Animation")]
	private Animator _stakeAnimator;

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
        _stakeAnimator = GetComponent<Animator>();
        _boneBend0 = transform.Find("Armature/AllRoot/Center/Stake/JointRoot/Bend0");
        _boneBend1 = _boneBend0.GetChild(0);
        _boneBend2 = _boneBend1.GetChild(0);
        _cappyBone = transform.Find(_cappyMountPath);
	}


	private void HandleBentAmount()
	{
		if (_bentAmount < _bentAmountMax && _bentAmount != -0.1f)
		{
			_bentAmount += 0.5f;
		}
		else if (_bentAmount != -0.1f)
		{
			// Stake extraído completamente
			GetComponent<Collider>().enabled = false;
			_stakeAnimator.Play("pullOut");
			MarioController.s.cappy.SetState(eStateCap.UnHack);
			_bentAmount = -0.1f;
		}
		else if (_stakeAnimator.GetBool("isDead"))
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
		Vector3 bendEuler = new Vector3(0, _bentVertical * _bentAmount, _bentHorizontal * _bentAmount);
		_boneBend0.localEulerAngles = bendEuler;
		_boneBend1.localEulerAngles = bendEuler;
		_boneBend2.localEulerAngles = bendEuler;
	}

	private void AdjustCappyOrientation()
	{
		_cappyBone.localRotation = Quaternion.LookRotation(Vector3.up, _cappyBone.up);
		_cappyBone.localEulerAngles = new Vector3(-_cappyBone.localEulerAngles.y, 0, 90);
	}
	
    public void OnCappyTrigger()
    {
        scr_main.s.capMountPoint = _cappyMountPath;
    }

    public void OnCappyHacked()
    {
        MarioController.s.cappy.SetTransformOffset(2, Vector3.zero, Vector3.zero);
        _stakeAnimator.Play("pull");
        _bentHorizontal = scr_manageInput.AxisDir(-1).x;
        _bentVertical = scr_manageInput.AxisDir(-1).y;
        this.enabled = true;
    }
}
