namespace Valuator.Services;

public interface IRabbitmqService
{
    void SendMessage( string id, CancellationTokenSource cts );
}
