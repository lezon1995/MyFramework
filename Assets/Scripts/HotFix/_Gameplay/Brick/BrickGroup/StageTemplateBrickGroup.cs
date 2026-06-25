namespace MarbleHero;

/// <summary>
/// 通过关卡模版生成砖块
/// </summary>
public class StageTemplateBrickGroup : BrickGroup, IArgs<ResourceRef<StageTemplate>>
{
    ResourceRef<StageTemplate> stageTemplate;

    public void onCreate(ResourceRef<StageTemplate> t)
    {
        stageTemplate = t;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        mResourceManager.unload(ref stageTemplate);
    }

    public override void buildBrickTemplates(int turnCount)
    {
        templates.Clear();
        templates.addRange(stageTemplate.getResource().bricks);
        for (var i = 0; i < templates.Count; i++)
        {
            var t = templates[i];
            t.health = 10;
            templates[i] = t;
        }
    }
}