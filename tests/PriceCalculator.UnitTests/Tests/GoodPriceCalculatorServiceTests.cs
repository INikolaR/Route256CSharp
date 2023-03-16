using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Microsoft.Extensions.Options;
using Moq;
using Route256.PriceCalculator.Domain;
using Route256.PriceCalculator.Domain.Entities;
using Route256.PriceCalculator.Domain.Models.PriceCalculator;
using Route256.PriceCalculator.Domain.Separated;
using Route256.PriceCalculator.Domain.Services;
using Route256.PriceCalculator.Domain.Services.Interfaces;
using Xunit;

namespace PriceCalculator.UnitTests.Tests;

public class GoodPriceCalculatorServiceTests
{
    [Fact]
    public void GoodPriceCalculatorService_WhenDefaultGoodId_ShouldThrow()
    {
        // Arrange
        var goodRepositoryMock = new Mock<IGoodsRepository>(MockBehavior.Default);
        var cut = new GoodPriceCalculatorService(goodRepositoryMock.Object, CreateDefaultService());
        const int goodId = default;
        const int distance = 10;

        // Act, Assert
        Assert.Throws<ArgumentException>(() => cut.CalculatePrice(goodId, distance));
    }

    [Fact]
    public void GoodPriceCalculatorService_WhenDefaultDistance_ShouldThrow()
    {
        // Arrange
        var goodRepositoryMock = new Mock<IGoodsRepository>(MockBehavior.Default);
        var cut = new GoodPriceCalculatorService(goodRepositoryMock.Object, CreateDefaultService());
        const int goodId = 1;
        const int distance = default;

        // Act, Assert
        Assert.Throws<ArgumentException>(() => cut.CalculatePrice(goodId, distance));
    }
    
    
    
    
    
    
    
    //==============================
    
    

    [Theory]
    [MemberData(nameof(CalcNotDefaultsMemberData))]
    public void PriceCalculatorService_WhenCalcNotDefaults_ShouldSuccess(
        int goodId,
        decimal distance,
        decimal expected)
    {
        // Arrange
        var options = new PriceCalculatorOptions
        {
            VolumeToPriceRatio = 1, 
        };

        var goodRepository = new GoodEntity[]
        {
            new ("Здоровенный ЯЗЬ", 1, 500, 100, 200, 4, 160, 100),
            new ("ЯЗЬ поменьше", 2, 200, 200, 100, 4, 160, 100),
            new ("Совсем маленький ЯЗЬ", 3, 100, 100, 200, 4, 160, 100)
        };
        
        var goodRepositoryMock = new Mock<IGoodsRepository>(MockBehavior.Default);
        var args = new List<int>();
        goodRepositoryMock
            .Setup(x => x.Get(goodId))
            .Returns(goodRepository[goodId - 1]);
        var storageRepositoryMock = new Mock<IStorageRepository>(MockBehavior.Default);
        var service = new PriceCalculatorService(CreateOptionsSnapshot(options).Value, storageRepositoryMock.Object);
        var cut = new GoodPriceCalculatorService(goodRepositoryMock.Object, service);
        
        // Act
        var result = cut.CalculatePrice(goodId, distance);

        // Assert
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> CalcNotDefaultsMemberData => CalcNotDefaults();
    private static IEnumerable<object[]> CalcNotDefaults()
    {
        yield return new object[]
        {
            1, 2, 20000
        };
        yield return new object[]
        {
            2, 2, 8000
        };
        yield return new object[]
        {
            3, 1, 2000
        };
    }
    
    private static IOptionsSnapshot<PriceCalculatorOptions> CreateOptionsSnapshot(
        PriceCalculatorOptions options)
    {
        var repositoryMock = new Mock<IOptionsSnapshot<PriceCalculatorOptions>>(MockBehavior.Strict);
        
        repositoryMock
            .Setup(x => x.Value)
            .Returns(() => options);

        return repositoryMock.Object;
    }
    
    private static IPriceCalculatorService CreateDefaultService()
    {
        var serviceMock = new Mock<IPriceCalculatorService>(MockBehavior.Default);
        serviceMock
            .Setup(x => x)
            .Returns(() => 0);
        return serviceMock.Object;
    }
}
