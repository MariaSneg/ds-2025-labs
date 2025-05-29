using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProtoKeyAPI.DTOs;
using ProtoKeyAPI.Services;

[Route( "api/[controller]" )]
[ApiController]
public class ProtoKeyController : Controller
{
    private StorageService _storageService;
    public ProtoKeyController( StorageService storageService )
    {
        _storageService = storageService;
    }

    [HttpGet( "keys" )]
    public async Task<IActionResult> GetKeys( [FromQuery] string? prefix )
    {
        try
        {
            var keys = await _storageService.Keys( prefix );
            return Ok( keys );
        }
        catch ( ArgumentException ex )
        {
            return BadRequest( ex.Message );
        }
    }

    [HttpGet( "get/{key}" )]
    public async Task<IActionResult> GetValue( string key )
    {
        try
        {
            var value = await _storageService.Get( key );
            return Ok( value );
        }
        catch ( ArgumentException ex )
        {
            return BadRequest( ex.Message );
        }
    }

    [HttpPost( "set" )]
    public async Task<IActionResult> SetValue( [FromBody] SetDto request )
    {
        try
        {
            await _storageService.Set( request.Key, request.Value );
            return Ok();
        }
        catch ( ArgumentException ex )
        {
            return BadRequest( ex.Message );
        }
    }
}
