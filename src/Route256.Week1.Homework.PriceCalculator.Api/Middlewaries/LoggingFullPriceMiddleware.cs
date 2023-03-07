using System.Net;
using System.Text;
using FluentValidation;

namespace Route256.Week1.Homework.PriceCalculator.Api.Middlewaries;

/// <summary>
/// Класс для логгирования нового метода вычисления полной стоимости товара.
/// </summary>
public class LoggingFullPriceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingFullPriceMiddleware> _logger;

    public LoggingFullPriceMiddleware(
        RequestDelegate next,
        ILogger<LoggingFullPriceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Stream originalBody = context.Response.Body;

        try {
            // Так как мы не можем прочитать context.Response.Body напрямую, копируем его в memoryStream,
            // читаем и возвращаем обратно.
            using (var memStream = new MemoryStream()) {
                context.Response.Body = memStream; // Копируем context.Response.Body в memStream.
                
                await _next.Invoke(context);

                memStream.Position = 0;
                var responseBodyString = await new StreamReader(memStream).ReadToEndAsync(); // Читаем.

                memStream.Position = 0;
                await memStream.CopyToAsync(originalBody); // Копируем обратно.
                
                if (context.Request.Path.ToString().Contains("calculateFullPrice"))
                {
                    // Если путь, по которому вызывается запрос, содержит имя нашего нового метода,
                    // то мы должны логгировать его работу, иначе - нет.
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                    var requestHeadersString = new StringBuilder();
                    foreach (var header in context.Request.Headers)
                    {
                        requestHeadersString.Append(header); // Читаем headers.
                    }
                    var requestBodyString = await new StreamReader(context.Request.Body).ReadToEndAsync(); // Читаем body.
                    _logger.LogInformation( // Логгируем.
                        "\nRequest:\n" +
                        $"  Timestamp: {DateTime.Now}\n" +
                        $"  Url: {context.Request.Path}\n" +
                        $"  Headers: {requestHeadersString}\n" +
                        $"  Body: {requestBodyString}\n" + 
                        $"Response:\n" +
                        $"  Body: {responseBodyString}\n");
                }
            }
        } finally {
            context.Response.Body = originalBody;
        }
        
        
        
        
        
        
        
    }
}