namespace MoreMountains
{
    /// <summary>
    /// A simple static class that keeps track of layer names, holds ready to use layermasks for most common layers and layermasks combinations
    /// Of course if you happen to change the layer order or numbers, you'll want to udpate this class.
    /// </summary>
    public static class LayerManager
    {
        public const int Border = 14;
        public const int Obstacles = 14;
        public const int Ground = 13;
        public const int Player = 12;
        public const int Brick = 7;
        public const int MovingPlatform = 16;
        public const int Ball = 6;

        public const int Border_Mask = 1 << Border;
        public const int Obstacles_Mask = 1 << Obstacles;
        public const int Ground_Mask = 1 << Ground;
        public const int Player_Mask = 1 << Player;
        public const int Brick_Mask = 1 << Brick;
        public const int MovingPlatform_Mask = 1 << MovingPlatform;
        public const int Ball_Mask = 1 << Ball;
    }
}