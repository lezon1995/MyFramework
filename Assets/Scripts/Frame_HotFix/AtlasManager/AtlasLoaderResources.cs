using UnityEngine;
using UnityEngine.U2D;
using static FrameBaseHotFix;
using UObject = UnityEngine.Object;

// 用于UGUI的multi sprite管理
public class AtlasLoaderResources : AtlasLoaderBase
{
	protected override void baseUnloadAtlas(AtlasBase atlas, bool showError) 
	{
		if (atlas is UGUIAtlas uguiAtlas)
		{
			res.unloadFromResources(ref uguiAtlas.mSpriteAtlas, showError);
		}
		else if (atlas is TPAtlas tpAtlas)
		{
			res.unloadFromResources(ref tpAtlas.mTexture, showError);
		}
	}
	protected override CustomAsyncOperation baseLoadAtlasAsync(string atlasName, AssetLoadDoneCallback doneCallback)
	{
		if (atlasName.endWith(".png"))
		{
			return res.loadInResourceAsync<Sprite>(atlasName, doneCallback);
		}
		else
		{
			return res.loadInResourceAsync<SpriteAtlas>(atlasName, doneCallback);
		}
	}
	protected override UObject[] baseLoadSubResource(string atlasName, out UObject mainAsset, bool errorIfNull) 
	{
		if (atlasName.endWith(".png"))
		{
			return res.loadSubFromResources<Sprite>(atlasName, out mainAsset, errorIfNull);
		}
		else
		{
			return res.loadSubFromResources<SpriteAtlas>(atlasName, out mainAsset, errorIfNull);
		}
	}
}