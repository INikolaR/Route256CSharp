using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Extensions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Xunit;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.HandlersTests;

public class ClearHistoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_MakeAllCalls()
    {
        //arrange
        var userId = Create.RandomId();

        var command = ClearHistoryCommandFaker.Generate()
            .WithUserId(userId)
            .WithCalculationIds(Array.Empty<long>());

        var builder = new ClearHistoryHandlerBuilder();
        builder.CalculationService
            .SetupCalculationsBelongToAnotherUser(command.CalculationIds)
            .SetupAbsentCalculations(command.CalculationIds)
            .SetupClearHistory();

        var handler = builder.Build();
        
        //act
        var result = await handler.Handle(command, default);

        //asserts
        handler.CalculationService
            .VerifyCalculationsBelongToAnotherUserWasCalledOnce(command)
            .VerifyAbsentCalculationsWasCalledOnce(command)
            .VerifyClearHistoryWasCalledOnce(command);
    }
    
    [Fact]
    public async Task Handle_AbsentCalculations_ShouldThrow()
    {
        //arrange
        var userId = Create.RandomId();

        var command = ClearHistoryCommandFaker.Generate()
            .WithUserId(userId)
            .WithCalculationIds(Array.Empty<long>());

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
            .VerifyCalculationsBelongToAnotherUserWasCalledOnce(command)
            .VerifyAbsentCalculationsWasCalledOnce(command)
            .VerifyClearHistoryWasCalledOnce(command);
    }
}