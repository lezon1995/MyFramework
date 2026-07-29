namespace MoreMountains;

public partial class Exordium
{
    protected override void initializePhases()
    {
        screen = CurrentScreen.PHASE;
        _phases[DungeonPhaseType.SELECT_CHARACTER] = new SelectCharacterPhase(this);
        _phases[DungeonPhaseType.SELECT_WEAPON] = new SelectWeaponPhase(this);
        _phases[DungeonPhaseType.SELECT_DIFFICULTY] = new SelectDifficultyPhase(this);
    }
}