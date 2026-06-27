#nullable enable

using RtsEngine.Math;
using RtsEngine.Structures;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildHelper : MonoBehaviour
{
	[SerializeField] private PrefabManager _prefabManager = null!;

	private GameObject? _dummyStructure = null!;
	private StructureType _structureType;

    void Start()
    {
		Close();
    }

    void Update()
    {
		if (_dummyStructure == null) return;

		UpdateDummy();
    }

	public void Open(StructureType structureType)
	{
		Debug.Log("Opened build helper");
		_structureType = structureType;

		BaseStructure dummyStructure = GetDummyStructureAtMousePos();
		_dummyStructure = Instantiate(_prefabManager.GetCorrespondingPrefab(dummyStructure), PrefabManager.GetInstanceCoordinates(dummyStructure), Quaternion.identity);

		StructureAnimationController animationController = _dummyStructure.GetComponent<StructureAnimationController>();
		animationController.Entity = dummyStructure;
		animationController.DisableDestroyAnimations = true;

		Debug.Log("instantiated");
		Debug.Log(_dummyStructure);

		SetTransparent(_dummyStructure);
	}

	private void UpdateDummy()
	{
		BaseStructure dummyStructure = GetDummyStructureAtMousePos();

		_dummyStructure!.transform.position = PrefabManager.GetInstanceCoordinates(dummyStructure);
	}

	private BaseStructure GetDummyStructureAtMousePos()
	{
		Vector2 mousePos = Mouse.current.position.ReadValue();
		Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

		Vec2 pos = WorldStateManager.Vector2ToVec2(worldMousePos);
		Vec2Int startPos = WorldStateManager.Instance.LatestState!.Map.CellPosFromWorldSpace(pos);

		return BaseStructure.FromType(_structureType, WorldStateManager.Instance.PlayerId, startPos);
	}

	private void SetTransparent(GameObject obj)
	{
		SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
		Color modifiedColor = renderer.color;
		modifiedColor.a = 0.5f;
		renderer.color = modifiedColor;
	}

	public void Close()
	{
		if (_dummyStructure != null)
		{
			Destroy(_dummyStructure);
		}
		_dummyStructure = null;
	}
}
