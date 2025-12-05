// 游戏常量定义

public static class HotfixDefine
{
    public const string GAMEPLAY_PATH = "_Gameplay";
    
    
    public const int BALL_LAYER = 6;
    public const int BALL_LAYER_MASK = 1 << BALL_LAYER;
    public const int BRICK_LAYER = 7;
    public const int BRICK_LAYER_MASK = 1 << BRICK_LAYER;
    public const int BORDER_LAYER = 8;
    public const int BORDER_LAYER_MASK = 1 << BORDER_LAYER;
    
    public const string BORDER_TOP_TAG = "BorderTop";
    public const string BORDER_BOT_TAG = "BorderBot";
    public const string BORDER_LEFT_TAG = "BorderLeft";
    public const string BORDER_RIGHT_TAG = "BorderRight";
    
}