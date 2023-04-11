using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualBasic;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;
using Route256.Week5.Homework.PriceCalculator.Dal.Repositories.Interfaces;
using Route256.Week5.Homework.PriceCalculator.IntegrationTests.Fixtures;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Route256.Week5.Homework.TestingInfrastructure.Fakers;
using Xunit;
using Xunit.Abstractions;

namespace Route256.Week5.Homework.PriceCalculator.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class CalculationsRepositoryTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly double _requiredDoublePrecision = 0.00001d;
    private readonly decimal _requiredDecimalPrecision = 0.00001m;
    private readonly TimeSpan _requiredDateTimePrecision = TimeSpan.FromMilliseconds(100);
    
    private readonly ICalculationRepository _calculationRepository;

    public CalculationsRepositoryTests(TestFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _calculationRepository = fixture.CalculationRepository;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Add_Calculations_Success(int count)
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(count)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        
        // Act
        var calculationIds = await _calculationRepository.Add(calculations, default);

        // Asserts
        calculationIds.Should().HaveCount(count);
        calculationIds.Should().OnlyContain(x => x > 0);
    }
    
    [Fact]
    public async Task Query_SingleCalculation_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate()
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        var expected = calculations.Single();
        
        var calculationId = (await _calculationRepository.Add(calculations, default))
            .Single();

        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 1, 0), 
            default);

        // Asserts
        foundCalculations.Should().HaveCount(1);
        var calculation = foundCalculations.Single();

        calculation.Id.Should().Be(calculationId);
        calculation.UserId.Should().Be(expected.UserId);
        calculation.At.Should().BeCloseTo(expected.At, _requiredDateTimePrecision);
        calculation.Price.Should().BeApproximately(expected.Price, _requiredDecimalPrecision);
        calculation.GoodIds.Should().BeEquivalentTo(expected.GoodIds);
        calculation.TotalVolume.Should().BeApproximately(expected.TotalVolume, _requiredDoublePrecision);
        calculation.TotalWeight.Should().BeApproximately(expected.TotalWeight, _requiredDoublePrecision);
    }
    
    [Theory]
    [InlineData(3,  2, 3)]
    [InlineData(1,  6, 1)]
    [InlineData(2,  8, 2)]
    [InlineData(3, 10, 0)]
    public async Task Query_CalculationsInRange_Success(int take, int skip, int expectedCount)
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(10)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        await _calculationRepository.Add(calculations, default);

        var allCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 100, 0), 
            default);
        
        var expected = allCalculations
            .OrderByDescending(x => x.At)
            .Skip(skip)
            .Take(take);
        
        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, take, skip), 
            default);

        // Asserts
        foundCalculations.Should().HaveCount(expectedCount);

        if (expectedCount > 0)
        {
            foundCalculations.Should().BeEquivalentTo(expected);
        }
    }
    
    [Fact]
    public async Task Query_AllCalculations_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        var calculationIds = (await _calculationRepository.Add(calculations, default))
            .ToHashSet();

        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 100, 0), 
            default);

        // Assert
        foundCalculations.Should().NotBeEmpty();
        foundCalculations.Should().OnlyContain(x => x.UserId == userId);
        foundCalculations.Should().OnlyContain(x => calculationIds.Contains(x.Id));
        foundCalculations.Should().BeInDescendingOrder(x => x.At);
    }
    
    [Fact]
    public async Task Query_Calculations_ReturnsEmpty_WhenForWrongUser()
    {
        // Arrange
        var userId = Create.RandomId();
        var anotherUserId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        var calculationIds = (await _calculationRepository.Add(calculations, default))
            .ToHashSet();

        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(anotherUserId, 100, 0), 
            default);

        // Assert
        foundCalculations.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData(1, new long[] {2, 3, 4})]
    [InlineData(2, new long[] {1, 3, 5})]
    [InlineData(3, new long[] {2, 3, 4, 10})]
    public async Task ConnectedGoodIdsQuery_Calculations_Correct(int userId, long[] calculationIds)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId).WithAt(now))
            .ToArray();

        await _calculationRepository.Add(calculations, default);
        
        var allCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 100, 0), 
            default);

        var filteredCalculations = allCalculations
            .Where(x => calculationIds.Contains(x.Id))
            .Select(x => x.GoodIds).ToArray();
        var expected = Array.Empty<long>();
        if (filteredCalculations.Length != 0)
            expected = filteredCalculations.Aggregate((x, y) => x.Concat(y).Distinct().ToArray());
        // Act
        var foundCalculations =
            await _calculationRepository.ConnectedGoodIdsQuery(
                new ClearHistoryCommandModel(userId, calculationIds),
                default);
        // Assert
        foundCalculations.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task AllConnectedGoodIdsQuery_Calculations_Correct()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId).WithAt(now))
            .ToArray();

        await _calculationRepository.Add(calculations, default);
        
        var allCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 100, 0), 
            default);

        var filteredCalculations = allCalculations
            .Select(x => x.GoodIds).ToArray();
        var expected = Array.Empty<long>();
        if (filteredCalculations.Length != 0)
            expected = filteredCalculations.Aggregate((x, y) => x.Concat(y).Distinct().ToArray());
        // Act
        var foundCalculations =
            await _calculationRepository.AllConnectedGoodIdsQuery(
                userId,
                default);
        // Assert
        foundCalculations.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task ClearHistory_Calculations_Correct()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId).WithAt(now))
            .ToArray();

        await _calculationRepository.Add(calculations, default);
        
        var allCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 100, 0), 
            default);
        var random = new Random();
        var calculationIds = allCalculations
            .Where(x => random.Next() % 2 == 0)
                .Select(c => c.Id)
            .ToArray();

        var filteredCalculations = allCalculations
            .Where(x => !calculationIds.Contains(x.Id)).ToArray();
        var expected = filteredCalculations;
        // Act
        await _calculationRepository.ClearHistory(calculationIds, default);
        var foundCalculations =
            await _calculationRepository.Query(
                new CalculationHistoryQueryModel(userId, 100, 0),
        default);
        // Assert
        foundCalculations.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task ClearAllHistory_Calculations_Correct()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId).WithAt(now))
            .ToArray();

        await _calculationRepository.Add(calculations, default);
        
        // Act
        await _calculationRepository.ClearAllHistory(userId, default);
        var foundCalculations =
            await _calculationRepository.Query(
                new CalculationHistoryQueryModel(userId, 100, 0),
                default);
        // Assert
        foundCalculations.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData(1, new long[] {2, 3, 4})]
    [InlineData(2, new long[] {1, 3, 5})]
    [InlineData(3, new long[] {2, 3, 4, 10})]
    public async Task CalculationsBelongToAnotherUser_Calculations_Correct(int userId, long[] calculationIds)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate(10)
            .Select(x => x.WithAt(now))
            .ToArray();

        await _calculationRepository.Add(calculations, default);

        for (var i = 0; i < calculations.Length; i++)
        {
            calculations[i] = calculations[i].WithId(i + 1);
        }
        var filteredCalculations = calculations
            .Where(x => x.UserId != userId)
            .Where(x => calculationIds.Contains(x.Id))
            .Select(x => x.Id).ToArray();
        var expected = filteredCalculations;
        // Act
        var foundCalculations =
            await _calculationRepository.CalculationsBelongToAnotherUser(
                new ClearHistoryCommandModel(userId, calculationIds),
                default);
        // Assert
        foundCalculations.Should().BeEquivalentTo(expected);
    }
    
    [Theory]
    [InlineData(new long[] {2, 3, 4})]
    [InlineData(new long[] {1, 3, 5})]
    [InlineData(new long[] {2, 3, 4, 10})]
    public async Task AbsentCalculations_Calculations_Correct(long[] calculationIds)
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;
        var calculations = CalculationEntityV1Faker.Generate(10)
            .Select(x => x.WithUserId(userId).WithAt(now))
            .ToArray();

        await _calculationRepository.Add(calculations, default);

        var allCalculations =
            await _calculationRepository.Query(new CalculationHistoryQueryModel(userId, 100, 0), default);
        var filteredCalculations = calculationIds
            .Where(x => !allCalculations.Select(y => y.Id).Contains(x))
            .ToArray();
        var expected = filteredCalculations;
        // Act
        var foundCalculations =
            await _calculationRepository.AbsentCalculations(
                new ClearHistoryCommandModel(userId, calculationIds),
                default);
        // Assert
        foundCalculations.Should().BeEquivalentTo(expected);
    }
}
