using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Route256.PriceCalculator.Domain.Exceptions;

namespace Route256.PriceCalculator.Api.ActionFilters;

public sealed class ExceptionFilterAttribute: Attribute, IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case DomainException exception:
                HandleBadRequest(context, exception);
                return;
                
            default:
                HandlerInternalError(context);
                return;
        }
    }

    private static void HandlerInternalError(ExceptionContext context)
    {
        var jsonResult = new JsonResult(new ErrorResponse(
            HttpStatusCode.InternalServerError, 
            "Возникла ошибка, уже чиним"));
        jsonResult.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Result = jsonResult;
    }

    private static void HandleBadRequest(ExceptionContext context, Exception exception)
    {
        var jsonResult = new JsonResult(
            new ErrorResponse(
            HttpStatusCode.BadRequest, 
            exception.Message));
        
        jsonResult.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Result = jsonResult;
    }
}