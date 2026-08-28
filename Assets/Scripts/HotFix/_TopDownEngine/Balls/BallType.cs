namespace MoreMountains
{
    enum WeaponType
    {
        BallGun,
        BallTurret,
    }
    
    public enum  BallType
    {
        NONE = 0,
        Normal = 10000,
        NormalFast = 10001,//移速+100%，伤害率50%
        NormalDouble = 10002,//一次攻击发射2个
        NormalPierce = 10003,//移速+100%，穿透1次，伤害-50%
        NormalTracking = 10004,
        NormalSequence = 10005,//连珠球，前5次攻击间隔为0.2秒，第6次攻击间隔为2秒
        NormalCrit = 10006,//每次撞击敌人，暴击率+20%，持续时间5秒
        NormalCritDuration = 10007,//每次暴击，持续时间+1秒
        NormalSpeedDamage = 10008,//每获得1%得弹道速度，伤害率+1%
        NormalShareStats = 10009,//此球共享同一个Ball属性，每拥有1个此球，该Ball属性值叠加

        #region 元素球

        //激光
        LaserBeam = 10100, //每次撞击会产生一道随机方向的激光束打击
        LaserBullet = 10101, //每次撞击会发射一枚激光子弹，穿透敌人，碰到障碍即销毁
        LaserBeam_V = 10102, //每次撞击会产生一道随机方向的激光束打击
        LaserBeam_H = 10103, //每次撞击会产生一道随机方向的激光束打击

        //冰
        IceFreeze = 10200, //对撞击的敌人施加0.5秒的冰冻
        IceFrost = 10201, //对撞击的敌人施加20%的减速，持续3秒
        
        //雷
        LightningStrike = 10300, //对场上1名敌人造成雷电打击
        LightningPath = 10301, //途经的路径

        //电
        ElectricityStrike = 10400, //对附近1名敌人造成电流打击
        Electrified = 10401, //对撞击的敌人施加1层【感电】，持续3秒，每次撞击特效会引爆一次感电伤害

        //岩
        RockQuake = 10500, //每次撞击会造成一次方形地震伤害
        RockSplash = 10501, //对撞击的敌人后方扇形区域造成溅射伤害，伤害为本次撞击伤害的30%

        //毒
        PoisonBurning = 10600, //每次撞击会施加1层【中毒】，最多3层，每层造成每秒造成5点伤害
        PoisonSplatter, //每次撞击溅射毒液落到附近地上，形成圆形区域，敌人在毒液区域上受到减速和和每秒1层的中毒效果

        //火
        FireBurning = 10700, //每次撞击会施加1层【灼烧】，最多3层，每层造成每秒造成5点伤害
        FireExplode = 10701, //每次撞击会造成一次圆形火焰爆炸伤害
        
        //水
        WaterSpot = 10800,//撞击一次后，在地面生成一摊水，持续3秒，敌人在上面减速30%

        //风
        Wind = 10900, //可以穿透敌人
        WindBreeze, //可以穿透敌人，对命中的敌人造成轻微击退，并减速20%，持续2秒
        WindStorm, //可以穿透敌人，会把附近的敌人不断拉向旋涡中心

        //裂变
        Fission = 11000, //每次撞击会分裂出2个小球
        FissionMini = 11001, //每次撞击会分裂出2个小球

        //复制
        Duplicate = 11100, //每次撞击有几率复制出一个球

        //飞弹
        Missile = 11200, //每次撞击会从玩家身后发射一道轨迹子弹在0.75秒后命中敌人
        
        //流血
        Bleed = 11300, //命中施加2层流血。每层流血将使敌人在受到弹珠攻击时额外承受1+0.1AP点伤害（最高8层）

        //金属
        Iron = 11400, //黑铁，100暴击率，移动速度-50%
        Lead = 11401,//铅铁，100暴击率，每次命中敌人时暴击率降低25%（最低0%）

        //光明（阳）
        Light, //命中时偷取目标2点护甲，附加自身50%护甲的物理伤害

        //黑暗（阴）
        Dark, //命中时偷取目标2点魔抗，附加自身50%魔抗的魔法伤害
        
        //创造
        Obstacle, //首次撞击会在原地创建一个障碍物，存活5秒

        //沙
        Sand, //每次撞击特效施加1层[沙环]，2层会引爆造成30点真实伤害

        //磁力
        Magnetic, //经过经验和金币附近会自动拾取

        //行星球
        Planet, //飞行方式是环绕自身

        //菱镜球
        Prism, //撞击携带元素Buff的敌人，会叠加1层该Buff

        //充能球
        Charge, //发射前需要经过2秒充能，充能期间玩家移动速度减少20%，充能后，飞行速度提高100%，伤害提高100%，发射后玩家受到击退，但每次撞击会减少10%飞行速度，10%的伤害，只能命中玩家时才能回收，回收时吸收球的剩余动能，受到击退

        //影子球
        Shadow, //进入砖块内部反弹，砖块阵亡后才会弹出

        #endregion


        #region 融合球

        LaserBeam_Crossed = 101000, //LaserBeam_V + LaserBeam_H 十字激光
        LaserBeam_Freeze, //LaserBeam + IceFreeze 冷冻激光，激光命中的单位全部冷冻0.5秒
        LaserBeam_Frost, //LaserBeam + IceFrost 霜冻激光，激光命中的单位全部施加20%的减速，持续3秒
        LaserBeam_Lightning, //LaserBeam + LightningStrike 雷电激光，激光命中的单位全部受到1次雷电打击
        LaserBeam_Electricity, //LaserBeam + ElectricityStrike 电流激光，激光命中的单位全部受到1次电流打击
        LaserBeam_RockQuake, //LaserBeam + RockQuake 岩震激光，激光的宽度变宽
        LaserBeam_RockSplash, //LaserBeam + RockSplash 溅射激光，命中后发射一道V型的激光
        LaserBeam_PoisonBurning, //LaserBeam + PoisonBurning 毒烧激光，激光命中的单位全部施加1层【中毒】
        LaserBeam_FireBurning, //LaserBeam + FireBurning 灼烧激光，激光命中的单位全部施加1层【中毒】
        LaserBeam_Wind, //LaserBeam + Wind 疾风激光，激光球可以穿透
        LaserBeam_Fission, //LaserBeam + Fission 裂变激光，激光球每次撞击有概率会分裂出1个激光子球，子球拥有父球50%的伤害
        LaserBeam_Duplicate, //LaserBeam + Duplicate 复制激光，激光球每次撞击有概率会复制1个自身
        LaserBeam_Missile, //LaserBeam + Missile 激光射线，类海克斯射线
        LaserBeam_Bleed, //LaserBeam + Bleed 流血射线，激光命中的单位全部施加2层流血
        LaserBeam_Light, //LaserBeam + Light 光明射线，激光命中的单位全部偷取目标2点护甲，附加自身50%护甲的物理伤害
        LaserBeam_Dark, //LaserBeam + Dark 黑暗射线，激光命中的单位全部偷取目标2点魔抗，附加自身50%魔抗的魔法伤害

        IceFreeze_Lightning = 102000, //IceFreeze + LightningStrike 冰雷打击，对场上随机1名敌人释放冰雷打击，对目标周围1米的范围内造成伤害和冰冻
        IceFreeze_Electricity, //IceFreeze + ElectricityStrike 冰电打击，对场上随机1名敌人释放冰电打击，对冰电经过的目标造成伤害和0.5秒的冰冻

        IceFrost_Lightning = 102010, //IceFrost + LightningStrike 霜雷打击，对场上随机1名敌人释放霜雷打击，在目标处生成一个持续3秒的3x3霜冻区域，对区域内的敌人每0.5秒受到伤害并施加1层霜冻，3层时冻住
        
        #endregion
    }

    /*public enum RelicType
    {
        A, //每次回收球时，会向最近的敌人发射一枚艾卡西亚暴雨导弹
        AA, //每次拾取金币时，会向最近的敌人发射一枚艾卡西亚暴雨导弹
        AAA, //激光子弹碰到障碍物会反弹一次后再销毁
        AAAA, //摧毁敌人后，有几率召唤1道可以在场景里无限反弹的激光子弹，持续4秒
        AAAC, //激光子弹穿透敌人时会引起爆炸，造成范围伤害，爆炸有0.5秒的CD
        AAAB, //闪电打击会造成雷电爆炸，造成范围伤害
        AAAD, //激光穿透效果会轻微击退敌人
        AAAE, //电流打击会额外传递给身边1个敌人
        B, //你造成的爆炸效果，会击退敌人
        C, //当球穿透敌人时，会触发一次脉冲伤害
        E, //每隔5秒会从自身发射一道冲击波，击退身边半径2米以内的敌人
        F, //每波开始前，会生成一个抵挡1次伤害的护盾
        G, //发射球时，有几率使其可穿透
        H, //发射球时，有几率使其可穿透，并改变球的飞行轨迹为绕着自身旋转360度后然后回收
    }*/
}