using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoBogus;
using FluentAssertions;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Bll.Services;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Extensions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Route256.Week5.Homework.TestingInfrastructure.Fakers;
using Xunit;
using Xunit.Abstractions;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.ServicesTests;

public class CalculationServiceTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CalculationServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task SaveCalculation_Success()
    {
        // arrange
        const int goodsCount = 5;
        
        var userId = Create.RandomId();
        var calculationId = Create.RandomId();
        
        var goodModels = GoodModelFaker.Generate(goodsCount)
            .ToArray();
        
        var goods = goodModels
            .Select(x => GoodEntityV1Faker.Generate().Single()
                .WithUserId(userId)
                .WithHeight(x.Height)
                .WithWidth(x.Width)
                .WithLength(x.Length)
                .WithWeight(x.Weight))
            .ToArray();
        var goodIds = goods.Select(x => x.Id)
            .ToArray();

        var calculationModel = CalculationModelFaker.Generate()
            .Single()
            .WithUserId(userId)
            .WithGoods(goodModels);
        
        var calculations = CalculationEntityV1Faker.Generate(1)
            .Select(x => x
                .WithId(calculationId)
                .WithUserId(userId)
                .WithPrice(calculationModel.Price)
                .WithTotalWeight(calculationModel.TotalWeight)
                .WithTotalVolume(calculationModel.TotalVolume))
            .ToArray();
        
        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupAddCalculations(new [] { calculationId })
            .SetupCreateTransactionScope();
        builder.GoodsRepository
            .SetupAddGoods(goodIds);

        var service = builder.Build();

        // act
        var result = await service.SaveCalculation(calculationModel, default);

        // assert
        result.Should().Be(calculationId);
        service.CalculationRepository
            .VerifyAddWasCalledOnce(calculations)
            .VerifyCreateTransactionScopeWasCalledOnce(IsolationLevel.ReadCommitted);
        service.GoodsRepository
            .VerifyAddWasCalledOnce(goods);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public void CalculatePriceByVolume_Success()
    {
        // arrange
        var goodModels = GoodModelFaker.Generate(5)
            .ToArray();
        
        var builder = new CalculationServiceBuilder();
        var service = builder.Build();

        //act
        var price = service.CalculatePriceByVolume(goodModels, out var volume);

        //asserts
        volume.Should().BeApproximately(goodModels.Sum(x => x.Height * x.Width * x.Length), 1e-9d);
        price.Should().Be((decimal)volume * CalculationService.VolumeToPriceRatio);
    }
    
    [Fact]
    public void CalculatePriceByWeight_Success()
    {
        // arrange
        var goodModels = GoodModelFaker.Generate(5)
            .ToArray();
        
        var builder = new CalculationServiceBuilder();
        var service = builder.Build();

        //act
        var price = service.CalculatePriceByWeight(goodModels, out var weight);

        //asserts
        weight.Should().Be(goodModels.Sum(x => x.Weight));
        price.Should().Be((decimal)weight * CalculationService.WeightToPriceRatio);
    }
    
    [Fact]
    public async Task QueryCalculations_Success()
    {
        // arrange
        var userId = Create.RandomId();

        var filter = QueryCalculationFilterFaker.Generate()
            .WithUserId(userId);
        
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId))
            .ToArray();

        var queryModel = CalculationHistoryQueryModelFaker.Generate()
            .WithUserId(userId)
            .WithLimit(filter.Limit)
            .WithOffset(filter.Offset);
        
        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupQueryCalculation(calculations);
        var service = builder.Build();

        //act
        var result = await service.QueryCalculations(filter, default);

        //asserts
        service.CalculationRepository
            .VerifyQueryWasCalledOnce(queryModel);
        
        service.VerifyNoOtherCalls();

        result.Should().NotBeEmpty();
        result.Should().OnlyContain(x => x.UserId == userId);
        result.Should().OnlyContain(x => x.Id > 0);
        result.Select(x => x.TotalWeight)
            .Should().IntersectWith(calculations.Select(x => x.TotalWeight));
        result.Select(x => x.TotalVolume)
            .Should().IntersectWith(calculations.Select(x => x.TotalVolume));
        result.Select(x => x.Price)
            .Should().IntersectWith(calculations.Select(x => x.Price));
    }
    
    [Fact]
    public async Task ClearHistory_Service_Success()
    {
        // arrange
        Random r = new Random();
        var numberOfElems = r.Next(1, 10);
        var calculationIds = new long[numberOfElems];
        var connectedGoodIds = new long[numberOfElems];
        for (int i = 0; i < numberOfElems; i++)
        {
            calculationIds[i] = Create.RandomId();
            connectedGoodIds[i] = Create.RandomId();
        }

        var model = ClearHistoryModelFaker.Generate()
            .WithConnectedGoodIds(connectedGoodIds)
            .WithCalculationIds(calculationIds);

        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupClearHistory()
            .SetupCreateTransactionScope();
        builder.GoodsRepository
            .SetupClearHistory();
        var service = builder.Build();

        //act
        await service.ClearHistory(model, default);

        //asserts
        service.CalculationRepository
            .VerifyClearHistoryWasCalledOnce(model.CalculationIds);

    }
    
    [Fact]
    public async Task CalculationsBelongToAnotherUser_Service_Success()
    {
        // arrange
        var userId = Create.RandomId();
        Random r = new Random();
        var numberOfElems = r.Next(1, 10);
        var calculationIds = new long[numberOfElems];
        for (int i = 0; i < numberOfElems; i++)
        {
            calculationIds[i] = Create.RandomId();
        }

        var model = QueryModelFaker.Generate()
            .WithUserId(userId)
            .WithCalculationIds(calculationIds);

        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupCalculationsBelongToAnotherUser(calculationIds)
            .SetupCreateTransactionScope();
        var service = builder.Build();

        //act
        await service.CalculationsBelongToAnotherUser(model, default);

        //asserts
        service.CalculationRepository
            .VerifyCalculationsBelongToAnotherUserWasCalledOnce(
                new ClearHistoryCommandModel(model.UserId, model.CalculationIds));

    }
    
    [Fact]
    public async Task AbsentCalculations_Service_Success()
    {
        // arrange
        var userId = Create.RandomId();
        Random r = new Random();
        var numberOfElems = r.Next(1, 10);
        var calculationIds = new long[numberOfElems];
        for (int i = 0; i < numberOfElems; i++)
        {
            calculationIds[i] = Create.RandomId();
        }

        var model = QueryModelFaker.Generate()
            .WithUserId(userId)
            .WithCalculationIds(calculationIds);

        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupAbsentCalculations(calculationIds)
            .SetupCreateTransactionScope();
        var service = builder.Build();

        //act
        await service.AbsentCalculations(model, default);

        //asserts
        service.CalculationRepository
            .VerifyAbsentCalculationsWasCalledOnce(
                new ClearHistoryCommandModel(model.UserId, model.CalculationIds));

    }
}
