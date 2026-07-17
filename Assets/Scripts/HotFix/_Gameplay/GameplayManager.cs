namespace MoreMountains;

public class GameplayManager : FrameSystem
{
    IDmgCalculator dmgCalculator;

    public int curPhase = 1;

    public override void init()
    {
        base.init();
        dmgCalculator = DmgCalculator.Default;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
    }

    public override void destroy()
    {
        base.destroy();
    }

    public void refreshPhase(int phase)
    {
        curPhase = phase;
        var brickGrid = brickManager.brickLayout;
        brickGrid.getCellSize(out var cellSize);

        var borderLeftX = levelManager.getDefaultBorderLeftX();
        var borderRightX = levelManager.getDefaultBorderRightX();

        var defaultWidth = abs(borderLeftX - borderRightX);
        switch (phase)
        {
            case 1:
                levelManager.moveBorderLeftBy(-cellSize.x * 0);
                levelManager.moveBorderRightBy(cellSize.x * 0);
                brickGrid.setWidth(defaultWidth + cellSize.x * 0 * 2);
                brickGrid.setCols(6);
                break;
            case 2:
                levelManager.moveBorderLeftBy(-cellSize.x * 1);
                levelManager.moveBorderRightBy(cellSize.x * 1);
                brickGrid.setWidth(defaultWidth + cellSize.x * 1 * 2);
                brickGrid.setCols(8);
                break;
            case 3:
                levelManager.moveBorderLeftBy(-cellSize.x * 2);
                levelManager.moveBorderRightBy(cellSize.x * 2);
                brickGrid.setWidth(defaultWidth + cellSize.x * 2 * 2);
                brickGrid.setCols(10);
                break;
            case 4:
                levelManager.moveBorderLeftBy(-cellSize.x * 3);
                levelManager.moveBorderRightBy(cellSize.x * 3);
                brickGrid.setWidth(defaultWidth + cellSize.x * 3 * 2);
                brickGrid.setCols(12);
                break;
        }

        brickGrid.getGrids();
    }
}