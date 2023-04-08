using Microsoft.AspNetCore.Mvc;

namespace Route256.Week5.Homework.PriceCalculator.Api.ActionFilters;

public class ResponseTypeAttribute : ProducesResponseTypeAttribute
{
    public ResponseTypeAttribute(int statusCode) 
        : base(typeof(ErrorResponse), statusCode)
    {
        
    }
}