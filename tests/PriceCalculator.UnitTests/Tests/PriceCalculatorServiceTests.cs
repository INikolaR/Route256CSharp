using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Microsoft.Extensions.Options;
using Moq;
using Route256.PriceCalculator.Api.Bll;
using Route256.PriceCalculator.Api.Bll.Models.PriceCalculator;
using Route256.PriceCalculator.Api.Bll.Services;
using Route256.PriceCalculator.Api.Dal.Entities;
using Route256.PriceCalculator.Api.Dal.Repositories.Interfaces;
using Xunit;

namespace Workshop.UnitTests;

public class PriceCalculatorServiceTests
{
    [Fact]
    public void PriceCalculatorService_WhenGoodsEmptyArray_ShouldThrow()
    {
        // Arrange
        var options = new PriceCalculatorOptions();
        var repositoryMock = new Mock<IStorageRepository>(MockBehavior.Default);
        var cut = new PriceCalculatorService(CreateOptionsSnapshot(options), repositoryMock.Object);
        var goods = Array.Empty<GoodModel>();

        // Act, Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => cut.CalculatePrice(goods));
    }
    
    [Fact]
    public void PriceCalculatorService_WhenCalcAny_ShouldSave()
    {
        StorageEntity storageEntity = null;
        
        // Arrange
        var options = new PriceCalculatorOptions { VolumeToPriceRatio = 1, WeightToPriceRatio = 1 };
        var repositoryMock = new Mock<IStorageRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.Save(It.IsAny<StorageEntity>()))
            .Callback<StorageEntity>(x => storageEntity = x);
        var cut = new PriceCalculatorService(CreateOptionsSnapshot(options), repositoryMock.Object);
        var goods = new Fixture().CreateMany<GoodModel>().ToArray();

        // Act
        var result = cut.CalculatePrice(goods);
        
        // Assert
        Assert.NotNull(storageEntity);
        repositoryMock.Verify(x => x.Save(It.IsAny<StorageEntity>()));
        repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(10, 10)]
    [InlineData(0, 0)]
    //[InlineData(-1, -1)]
    public void PriceCalculatorService_WhenCalcByVolume_ShouldSuccess(
        decimal volumeToPriceRatio,
        decimal expected)
    {
        // Arrange
        var options = new PriceCalculatorOptions
        {
            VolumeToPriceRatio = volumeToPriceRatio
        };

        var repositoryMock = CreateRepositoryMock();
        var cut = new PriceCalculatorService(CreateOptionsSnapshot(options), repositoryMock.Object);
        var good = new GoodModel(10, 10, 10, 0);

        // Act
        var result = cut.CalculatePrice(new[] { good });

        // Assert
        Assert.Equal(expected, result);
    }

    private static Mock<IStorageRepository> CreateRepositoryMock()
    {
        var repositoryMock = new Mock<IStorageRepository>(MockBehavior.Strict);
        repositoryMock.Setup(x => x.Save(It.IsAny<StorageEntity>()));
        return repositoryMock;
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

    [Theory]
    [MemberData(nameof(CalcByVolumeManyMemberData))]
    public void PriceCalculatorService_WhenCalcByVolumeMany_ShouldSuccess(
        GoodModel[] goods,
        decimal expected)
    {
        // Arrange
        var options = new PriceCalculatorOptions
        {
            VolumeToPriceRatio = 1, 
        };
        var repositoryMock = CreateRepositoryMock();
        var cut = new PriceCalculatorService(CreateOptionsSnapshot(options), repositoryMock.Object);
        
        // Act
        var result = cut.CalculatePrice(goods);

        // Assert
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> CalcByVolumeManyMemberData => CalcByVolumeMany();
    private static IEnumerable<object[]> CalcByVolumeMany()
    {
        yield return new object[]
        {
            new GoodModel[] { new(10, 10, 10, 0), }, 1
        };

        yield return new object[]
        {
            Enumerable
                .Range(1, 2)
                .Select(x => new GoodModel(10, 10, 10, 0))
                .ToArray(),
            2
        };
    }
}