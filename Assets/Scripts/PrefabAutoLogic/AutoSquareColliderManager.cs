using RtsEngine.EntityProperties;
using RtsEngine.Structures;
using UnityEngine;

public class AutoSquareColliderManager : MonoBehaviour
{
	public const float padding = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		SpriteRenderer renderer = this.gameObject.GetComponent<SpriteRenderer>();
		Sprite sprite = renderer.sprite;
		Vector2 spriteSize = sprite.bounds.size;

		BoxCollider2D collider = this.gameObject.GetComponent<BoxCollider2D>();
		// collider.offset = new Vector2(minSize / 2, minSize / 2);

		Entity entity = WorldStateManager.Instance.GetEntity(this.gameObject);
		if (entity != null)
		{
			if (entity is BaseStructure structure)
			{
				collider.offset = new Vector2((float)structure.Width / 2f, (float)structure.Height / 2f);
				collider.size = new Vector2(structure.Width, structure.Height);
			}
		}
		else
		{
			collider.offset = Vector2.zero;
			collider.size = spriteSize;
		}

		// minSize -= padding;
		// collider.size = new Vector2(minSize, minSize);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
