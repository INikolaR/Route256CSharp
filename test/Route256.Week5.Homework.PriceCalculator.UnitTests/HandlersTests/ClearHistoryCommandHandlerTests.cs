using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Extensions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Xunit;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.HandlersTests;

public class ClearHistoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_MakeAllCalls_EmptyCalculationIds()
    {
        //arrange
        var userId = Create.RandomId();
        Random r = new Random();
        var numberOfElems = r.Next(1, 10);
        var connectedGoodIds = new long[numberOfElems];
        for (int i = 0; i < numberOfElems; i++)
        {
            connectedGoodIds[i] = Create.RandomId();
        }

        var command = ClearHistoryCommandFaker.Generate()
            .WithUserId(userId)
            .WithCalculationIds(Array.Empty<long>());

        var model = new QueryModel(command.UserId, command.CalculationIds);

        var builder = new ClearHistoryHandlerBuilder();
        builder.CalculationService
            .SetupCalculationsBelongToAnotherUser(Array.Empty<long>())
            .SetupAbsentCalculations(Array.Empty<long>())
            .SetupAllConnectedGoodIdsQuery(connectedGoodIds)
            .SetupClearAllHistory();

        var handler = builder.Build();
        
        //act
        var result = await handler.Handle(command, default);

        //asserts
        handler.CalculationService
            .VerifyCalculationsBelongToAnotherUserWasCalledOnce(model)
            .VerifyAbsentCalculationsWasCalledOnce(model)
            .VerifyAllConnectedGoodIdsQueryWasCalledOnce(userId)
            .VerifyClearAllHistoryWasCalledOnce(new ClearAllHistoryModel(userId, connectedGoodIds));
    }
    
    [Fact]
    public async Task Handle_MakeAllCalls_NotEmptyCalculationIds()
    {
        //arrange
        var userId = Create.RandomId();
        Random r = new Random();
        var numberOfElems = r.Next(1, 10);
        var connectedGoodIds = new long[numberOfElems];
        for (int i = 0; i < numberOfElems; i++)
        {
            connectedGoodIds[i] = Create.RandomId();
        }

        var command = ClearHistoryCommandFaker.Generate()
            .WithUserId(userId)
            .WithCalculationIds(new long[] {1});

        var model = new QueryModel(command.UserId, command.CalculationIds);

        var builder = new ClearHistoryHandlerBuilder();
        builder.CalculationService
            .SetupCalculationsBelongToAnotherUser(Array.Empty<long>())
            .SetupAbsentCalculations(Array.Empty<long>())
            .SetupConnectedGoodIdsQuery(connectedGoodIds)
            .SetupClearHistory();

        var handler = builder.Build();
        
        //act
        var result = await handler.Handle(command, default);

        //asserts
        handler.CalculationService
            .VerifyCalculationsBelongToAnotherUserWasCalledOnce(model)
            .VerifyAbsentCalculationsWasCalledOnce(model)
            .VerifyConnectedGoodIdsQueryWasCalledOnce(model)
            .VerifyClearHistoryWasCalledOnce(new ClearHistoryModel(connectedGoodIds, command.CalculationIds));
    }
    
    [Fact]
    public async Task Handle_AbsentCalculations_ShouldThrow()
    {
        //arrange
        var userId = Create.RandomId();

        var command = ClearHistoryCommandFaker.Generate()
            .WithUserId(userId)
            .WithCalculationIds(Array.Empty<long>());
        
        var model = new QueryModel(command.UserId, command.CalculationIds);

        var builder = new ClearHistoryHandlerBuilder();
        builder.CalculationService
            .SetupCalculationsBelongToAnotherUser(command.CalculationIds)
            .SetupAbsentCalculationsThrows(command.CalculationIds)
            .SetupClearHistory();

        var handler = builder.Build();
        
        //act
        var act = () =>  handler.Handle(command, default);

        //asserts
        await Assert.ThrowsAsync<OneOrManyCalculationsNotFoundException>(act);
        handler.CalculationService
            .VerifyCalculationsBelongToAnotherUserWasCalledOnce(model)
            .VerifyAbsentCalculationsWasCalledOnce(model); }
    
    [Fact]
    public async Task Handle_CalculationsBelongToAnotherUsers_ShouldThrow()
    {
        //arrange
        var userId = Create.RandomId();

        var command = ClearHistoryCommandFaker.Generate()
            .WithUserId(userId)
            .WithCalculationIds(Array.Empty<long>());
        
        var model = new QueryModel(command.UserId, command.CalculationIds);

        var builder = new ClearHistoryHandlerBuilder();
        builder.CalculationService
            .SetupCalculationsBelongToAnotherUserThrows(command.CalculationIds)
            .SetupAbsentCalculations(command.CalculationIds)
            .SetupClearHistory();

        var handler = builder.Build();
        
        //act
        var act = () =>  handler.Handle(command, default);

        //asserts
        await Assert.ThrowsAsync<OneOrManyCalculationsBelongsToAnotherUserException>(act);
        handler.CalculationService
            .VerifyCalculationsBelongToAnotherUserWasCalledOnce(model)
            .VerifyAbsentCalculationsWasCalledOnce(model);
    }
}