using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Valuator.Attributes;

public class IsAuthorAsyncAttribute : TypeFilterAttribute
{
    public IsAuthorAsyncAttribute() : base( typeof( IsAuthorAsyncFilter ) )
    {
    }

    private class IsAuthorAsyncFilter : IAsyncAuthorizationFilter
    {
        private readonly IShardManager _shardManager;

        public IsAuthorAsyncFilter( IShardManager shardManager )
        {
            _shardManager = shardManager;
        }

        public async Task OnAuthorizationAsync( AuthorizationFilterContext context )
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            if ( !user.Identity.IsAuthenticated )
            {
                context.Result = new ChallengeResult();
                return;
            }

            var userClaim = user.FindFirst( ClaimTypes.Name );
            if ( userClaim == null )
            {
                SetErrorAndRedirect( context, "Вы не авторизованы" );
                return;
            }

            var routeValues = context.RouteData.Values;
            string id = routeValues[ "id" ]?.ToString();
            string username = userClaim.Value;

            string author = _shardManager.GetAuthor( id ).ToString();

            if ( author == null || username != author )
            {
                SetErrorAndRedirect( context, "Вы не имеете прав доступа" );
                return;
            }
        }

        private void SetErrorAndRedirect( AuthorizationFilterContext context, string message )
        {
            var tempData = context.HttpContext.RequestServices
                .GetService( typeof( ITempDataDictionaryFactory ) ) as ITempDataDictionaryFactory;

            var tempDataDict = tempData?.GetTempData( context.HttpContext );
            if ( tempDataDict != null )
            {
                tempDataDict[ "AuthError" ] = message;
            }

            context.Result = new RedirectToPageResult( "/Error/AccessDenied" );
        }
    }
}


