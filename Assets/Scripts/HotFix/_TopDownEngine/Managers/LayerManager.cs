namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A simple static class that keeps track of layer names, holds ready to use layermasks for most common layers and layermasks combinations
    /// Of course if you happen to change the layer order or numbers, you'll want to udpate this class.
    /// </summary>
    public static class LayerManager
    {
        public const int Obstacles = 8;
        public const int Ground = 9;
        public const int Player = 10;
        public const int Enemies = 13;
        public const int Hole = 15;
        public const int MovingPlatform = 16;
        public const int FallingPlatform = 17;
        public const int Projectile = 18;

        public const int Obstacles_Mask = 1 << Obstacles;
        public const int Ground_Mask = 1 << Ground;
        public const int Player_Mask = 1 << Player;
        public const int Enemies_Mask = 1 << Enemies;
        public const int Hole_Mask = 1 << Hole;
        public const int MovingPlatform_Mask = 1 << MovingPlatform;
        public const int FallingPlatform_Mask = 1 << FallingPlatform;
        public const int Projectile_Mask = 1 << Projectile;
    }
}