using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scr_behaviorMoon : MonoBehaviour
{
    [SerializeField] private int CurrentState = 0;
    public string MoonName = "ERROR";
    public int MoonColor = 0;

    private float _rotateAddition = 0f;
    private Animator _animator;
    private AudioSource _audioSource;
    private SkinnedMeshRenderer _meshRenderer;

    #region Unity Methods
    private void Start()
    {
        InitializeComponents();
        ApplyMoonColor();
        scr_manAudio.s.PlaySelfSND(ref _audioSource, eSnd.MoonNearby, true, false, 0.6f);
    }

    private void Update()
    {
        if (CurrentState == 0) HandleRotation();
        else if (CurrentState == 1) HandleCollectedState();
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _meshRenderer = transform.GetChild(1).GetChild(0).GetComponent<SkinnedMeshRenderer>();
    }
    #endregion

    #region Moon Behavior
    private void HandleRotation()
    {
        if (_rotateAddition > 0) _rotateAddition -= 15f;
        transform.Rotate(0, (-150f - _rotateAddition) * Time.deltaTime, 0);
    }

    private void HandleCollectedState()
    {
        if (!scr_manAudio.s.isPlaying(false))
        {
            MarioEvent.s.SetEvent(eEventPl.demoMoon, 2);
            Destroy(gameObject);
        }
    }

    private void ApplyMoonColor()
    {
        _meshRenderer.material.SetColor("_Color", GetMoonColor(MoonColor));
        _meshRenderer.material.SetColor("_SpecColor", GetMoonFresnelColor(MoonColor));
    }

    private Color GetMoonColor(int moonColorIndex)
    {
        switch (moonColorIndex)
        {
            case 0: return new Color(0.91f, 0.9f, 0.2f, 1);
            case 1: return new Color(0.6f, 0.33f, 0.26f, 1);
            case 2: return new Color(0.11f, 0.25f, 0.9f, 1);
            case 3: return new Color(0.255f, 0.8f, 0.85f, 1);
            case 4: return new Color(0.24f, 0.87f, 0.4f, 1);
            case 5: return new Color(0.9f, 0.45f, 0.18f, 1);
            case 6: return new Color(0.82f, 0.165f, 0.192f, 1);
            case 7: return new Color(0.94f, 0.584f, 0.58f, 1);
            case 8: return new Color(0.74f, 0.48f, 0.945f, 1);
            case 9: return new Color(0.9f, 0.85f, 0.666f, 1);
            default: return Color.white;
        }
    }

	private Color GetMoonFresnelColor(int moonFresnelIndex)
	{
		switch (moonFresnelIndex)
		{
			case 0:
			case 4:
			case 5: return new Color(0.91f, 0.9f, 0.2f, 1);
			case 7: return new Color(0.94f, 0.584f, 0.58f, 1);
			case 8: return new Color(0.74f, 0.48f, 0.945f, 1);
			case 9: return new Color(0.9f, 0.85f, 0.666f, 1);
			default: return Color.white;
		}
	}
    #endregion

    #region Interaction
    private void OnTouch(int touchType)
    {
        if (touchType == 1) _rotateAddition = 1200f;
        else if (touchType == 2) CollectMoon();
    }

    private void CollectMoon()
    {
        CurrentState = 1;
        DisableMoonComponents();
        AnimateAndMoveMoon();
        UpdateMoonUI();
    }

    private void DisableMoonComponents()
    {
        GetComponent<Collider>().enabled = false;
        _audioSource.enabled = false;
        MarioEvent.s.SetEvent(eEventPl.demoMoon);
    }

    private void AnimateAndMoveMoon()
    {
        _animator.Play("get");
        Transform marioTransform = MarioController.s.transform;
        transform.position = marioTransform.position;
        transform.rotation = marioTransform.rotation;
    }

    private void UpdateMoonUI()
    {
        Transform mainTransform = scr_main.s.transform.GetChild(1).GetChild(1);
        mainTransform.GetChild(1).GetComponent<Text>().text = MoonName;
        mainTransform.GetChild(2).GetComponent<Text>().text = System.DateTime.UtcNow.ToShortDateString();
    }
    #endregion
}
