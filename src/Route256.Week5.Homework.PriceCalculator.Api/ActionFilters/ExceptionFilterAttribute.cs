using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;

namespace Route256.Week5.Homework.PriceCalculator.Api.ActionFilters;

public class ExceptionFilterAttribute : Attribute, IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case OneOrManyCalculationsBelongsToAnotherUserException exception:
                HandleForbidden(context, exception);
                break;
            case OneOrManyCalculationsNotFoundException:
                HandleBadRequest(context);
                break;
        }
    }
    
    private static void HandleForbidden(
        ExceptionContext context,
        OneOrManyCalculationsBelongsToAnotherUserException exception)
    {
        var jsonResult = new JsonResult(new ErrorResponse(exception.WrongCalculationIds));
        jsonResult.StatusCode = (int)HttpStatusCode.Forbidden;
        context.Result = jsonResult;
    }

    private static void HandleBadRequest(
        ExceptionContext context)
    {
        var jsonResult = new JsonResult(new());
        jsonResult.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Result = jsonResult;
    }
}