using Microsoft.AspNetCore.SignalR;

namespace RankCalculator;

public class RankHub : Hub
{
    public async Task SendTextRank(double rank, string id)
    {
        await Clients.All.SendAsync( "Received", rank, id);
    }
}
