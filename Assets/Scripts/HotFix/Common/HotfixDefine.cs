// 游戏常量定义

public static class HotfixDefine
{
    public const string GAMEPLAY_PATH = "_Gameplay";


    public const int BALL_LAYER = 6;
    public const int BALL_LAYER_MASK = 1 << BALL_LAYER;
    public const int BRICK_LAYER = 7;
    public const int BRICK_LAYER_MASK = 1 << BRICK_LAYER;
    public const int BORDER_LEFT_LAYER = 8;
    public const int BORDER_RIGHT_LAYER = 9;
    public const int BORDER_TOP_LAYER = 10;
    public const int BORDER_BOT_LAYER = 11;
    public const int OBSTACLE_LAYER = 14;
    public const int BORDER_LEFT_LAYER_MASK = 1 << BORDER_LEFT_LAYER;
    public const int BORDER_RIGHT_LAYER_MASK = 1 << BORDER_RIGHT_LAYER;
    public const int BORDER_TOP_LAYER_MASK = 1 << BORDER_TOP_LAYER;
    public const int BORDER_BOT_LAYER_MASK = 1 << BORDER_BOT_LAYER;
    public const int OBSTACLE_LAYER_MASK = 1 << OBSTACLE_LAYER;
    public const int ALL_BORDER_LAYER_MASK = BORDER_LEFT_LAYER_MASK | BORDER_RIGHT_LAYER_MASK | BORDER_TOP_LAYER_MASK | BORDER_BOT_LAYER_MASK;

    public static BorderToBallDamageModifier BALL_IMMUNE_TO_BORDER_DAMAGE_MODIFIER = (ref int damage) =>
    {
        damage = 0;
        return false;
    };
}