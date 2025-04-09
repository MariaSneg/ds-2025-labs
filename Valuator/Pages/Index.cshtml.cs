using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Repositories;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IValuatorRepository _repository;
    private readonly IRabbitmqService _service;

    public string Port { get; private set; }

    public IndexModel( ILogger<IndexModel> logger, IValuatorRepository repository, IRabbitmqService rabbitmqService )
    {
        _logger = logger;
        _repository = repository;
        _service = rabbitmqService;
    }

    public void OnGet()
    {
        Port = Environment.GetEnvironmentVariable( "EXTERNAL_PORT" ) ?? "NO PORT";
    }

    public IActionResult OnPost(string text, CancellationTokenSource cts )
    {
        _logger.LogDebug( text );


        string id = Guid.NewGuid().ToString();

        int similarity = _repository.CheckSimilarity( text );
        _repository.AddSimilarity( id, similarity );

        _repository.AddText( id, text );

        _service.SendRankMessage( id, cts );
        _service.SendSimilarityMessage( id, similarity, cts );

        return Redirect( $"summary?id={id}" );
    }

    private double CalculateRank(string text)
    {
        if(string.IsNullOrEmpty(text)) 
            return 0;
        int totalCount = text.Length;

        int nonLetterCount = text.Count( c => !char.IsLetter(c));

        double rank = (nonLetterCount * 1.0) / totalCount;

        return Math.Round( rank, 3 );
    }
}
