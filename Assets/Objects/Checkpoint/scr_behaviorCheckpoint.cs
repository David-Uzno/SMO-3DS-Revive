using UnityEngine;

public class scr_behaviorCheckpoint : MonoBehaviour
{
    [Tooltip("Index of the point of appearance associated with this checkpoint.")]
    public int SpawnPointIndex = 0;

    [SerializeField]
    [Tooltip("Material that is applied after activating the checkpoint.")]
    private Material _materialAfter;

    private bool _wasActivated = false;
    private Animator _checkpointAnimator;

	private void Start()
	{
		InitializeComponents();
	}

    private void InitializeComponents()
    {
        _checkpointAnimator = GetComponent<Animator>();
    }

    private void OnTouch(int type)
    {
        if (type < 1) return;

        if (!_wasActivated)
        {
            ActivateCheckpoint();
        }

        HandleCheckpointAnimation(type);
    }

    private void ActivateCheckpoint()
    {
        Renderer checkpointRenderer = transform.GetChild(1).GetChild(1).GetComponent<Renderer>();
        checkpointRenderer.material = _materialAfter;

        scr_manageData.s.Save();
        _wasActivated = true;
    }

    private void HandleCheckpointAnimation(int type)
    {
        _checkpointAnimator.Play("get");

        if (type == 1)
        {
            // Acción específica para type == 1 (actualmente vacía).
        }
        else
        {
            // MarioController.s.SetAnim("takeCheckpoint", 0.1f, -1, true, true);
        }
    }
}
