using UnityEngine;

[ExecuteAlways]
public class SequenceSpritePreview : SequenceImagePreviewBase
{
#if UNITY_EDITOR
	protected SpriteRenderer mRenderer;
	public override void Awake()
	{
		base.Awake();
		mRenderer = GetComponentInChildren<SpriteRenderer>();
	}
	protected override Component getSpriteComponent()
	{
		return mRenderer;
	}
#endif
}
