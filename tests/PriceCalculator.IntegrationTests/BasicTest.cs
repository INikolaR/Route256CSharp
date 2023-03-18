using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Route256.PriceCalculator.Api.Controllers;
using Route256.PriceCalculator.Api.Requests.V3;
using Xunit.Abstractions;

namespace PriceCalculator.IntegrationTests;

public class BasicTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public BasicTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    [Fact]
    public async Task App_SwaggerIsWorking()
    {

        // Arrange
        var app = new AppFixture();
        var httpClient = app.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/swagger/index.html");
        
        // Assert
        response.EnsureSuccessStatusCode();
        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
    }
    
    [Fact]
    public void App_V3DeliveryPrice_ResponsesCorrectly()
    {

        // Arrange
        var app = new AppFixture();
        var request = new GoodCalculateRequest(1, 1);
        
        var controller = app.Services.GetRequiredService<V3DeliveryPriceController>();
        
        // Act
        var response = controller.Calculate(request);
        
        //Assert
        Assert.Equal(7280486.64m, response.Result.Price);
    }
    
    [Fact]
    public async Task App_V3DeliveryPrice_ThrowsExceptionCorrectly()
    {

        // Arrange
        var app = new AppFixture();
        var request = new GoodCalculateRequest(1, 0);
        
        var controller = app.Services.GetRequiredService<V3DeliveryPriceController>();

        //Act, Assert
        await Assert.ThrowsAsync<ArgumentException>(() => controller.Calculate(request));
    }
}