namespace Valuator.Services;

public interface IRabbitmqService
{
    void SendRankMessage( string id, CancellationTokenSource cts );
    void SendSimilarityMessage( string id, int similarity, CancellationTokenSource cts );
}
