/// <summary>Bar for the magnet power-up.</summary>
public class TimerController : PowerUpTimerBar
{
    protected override void Subscribe()
    {
        PlayerController.MagnetActivated += StartCountdown;
    }

    protected override void Unsubscribe()
    {
        PlayerController.MagnetActivated -= StartCountdown;
    }
}
