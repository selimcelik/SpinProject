public readonly struct DeathPopupDecisionSignal
{
    public readonly DeathPopupDecision Decision;

    public DeathPopupDecisionSignal(DeathPopupDecision decision)
    {
        Decision = decision;
    }
}

public enum DeathPopupDecision
{
    GiveUp = 0,
    Revive = 1,
}
