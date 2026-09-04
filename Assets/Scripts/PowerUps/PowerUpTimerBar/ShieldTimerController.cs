/// <summary>Bar for the shield power-up.</summary>
public class ShieldTimerController : PowerUpTimerBar
{
    protected override void Subscribe()
    {
        ShieldPowerUp.ShieldActivated += StartCountdown;
    }

    protected override void Unsubscribe()
    {
        ShieldPowerUp.ShieldActivated -= StartCountdown;
    }
}
