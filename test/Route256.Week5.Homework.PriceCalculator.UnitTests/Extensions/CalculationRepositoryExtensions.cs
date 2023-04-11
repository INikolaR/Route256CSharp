using System.Linq;
using System.Threading;
using System.Transactions;
using Moq;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Dal.Entities;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;
using Route256.Week5.Homework.PriceCalculator.Dal.Repositories.Interfaces;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Comparers;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Extensions;

public static class CalculationRepositoryExtensions
{
    public static Mock<ICalculationRepository> SetupAddCalculations(
        this Mock<ICalculationRepository> repository,
        long[] ids)
    {
        repository.Setup(p =>
                p.Add(It.IsAny<CalculationEntityV1[]>(), 
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);

        return repository;
    }
    
    public static Mock<ICalculationRepository> SetupCreateTransactionScope(
        this Mock<ICalculationRepository> repository)
    {
        repository.Setup(p =>
                p.CreateTransactionScope(It.IsAny<IsolationLevel>()))
            .Returns(new TransactionScope());

        return repository;
    }
    
    public static Mock<ICalculationRepository> SetupQueryCalculation(
        this Mock<ICalculationRepository> repository,
        CalculationEntityV1[] calculations)
    {
        repository.Setup(p =>
                p.Query(It.IsAny<CalculationHistoryQueryModel>(), 
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(calculations);

        return repository;
    }
    
    public static Mock<ICalculationRepository> SetupClearHistory(
        this Mock<ICalculationRepository> repository)
    {
        repository.Setup(p =>
                p.ClearHistory(It.IsAny<long[]>(), 
                    It.IsAny<CancellationToken>()));

        return repository;
    }
    
    public static Mock<ICalculationRepository> SetupCalculationsBelongToAnotherUser(
        this Mock<ICalculationRepository> repository,
        long[] ids)
    {
        repository.Setup(p =>
            p.CalculationsBelongToAnotherUser(It.IsAny<ClearHistoryCommandModel>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);

        return repository;
    }
    
    public static Mock<ICalculationRepository> SetupAbsentCalculations(
        this Mock<ICalculationRepository> repository,
        long[] ids)
    {
        repository.Setup(p =>
                p.AbsentCalculations(It.IsAny<ClearHistoryCommandModel>(), 
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);

        return repository;
    }

    public static Mock<IGoodsRepository> SetupClearHistory(
        this Mock<IGoodsRepository> repository)
    {
        repository.Setup(p =>
            p.ClearHistory(It.IsAny<long[]>(), 
                It.IsAny<CancellationToken>()));

        return repository;
    }
    public static Mock<ICalculationRepository> VerifyAddWasCalledOnce(
        this Mock<ICalculationRepository> repository,
        CalculationEntityV1[] calculations)
    {
        repository.Verify(p =>
                p.Add(
                    It.Is<CalculationEntityV1[]>(x => x.SequenceEqual(calculations, new CalculationEntityV1Comparer())),
                    It.IsAny<CancellationToken>()),
            Times.Once);

        return repository;
    }
    
    public static Mock<ICalculationRepository> VerifyQueryWasCalledOnce(
        this Mock<ICalculationRepository> repository,
        CalculationHistoryQueryModel query)
    {
        repository.Verify(p =>
                p.Query(
                    It.Is<CalculationHistoryQueryModel>(x => x == query),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
    
    public static Mock<ICalculationRepository> VerifyCreateTransactionScopeWasCalledOnce(
        this Mock<ICalculationRepository> repository,
        IsolationLevel isolationLevel)
    {
        repository.Verify(p =>
                p.CreateTransactionScope(
                    It.Is<IsolationLevel>(x => x == isolationLevel)),
            Times.Once);
        
        return repository;
    }
    
    public static Mock<ICalculationRepository> VerifyClearHistoryWasCalledOnce(
        this Mock<ICalculationRepository> repository,
        long[] calculationIds)
    {
        repository.Verify(p =>
                p.ClearHistory(
                    It.Is<long[]>(x => x.SequenceEqual(calculationIds)),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
    
    public static Mock<IGoodsRepository> VerifyClearHistoryWasCalledOnce(
        this Mock<IGoodsRepository> repository,
        long[] calculationIds)
    {
        repository.Verify(p =>
                p.ClearHistory(
                    It.Is<long[]>(x => x.SequenceEqual(calculationIds)),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
    
    public static Mock<ICalculationRepository> VerifyCalculationsBelongToAnotherUserWasCalledOnce(
        this Mock<ICalculationRepository> repository,
        ClearHistoryCommandModel model)
    {
        repository.Verify(p =>
                p.CalculationsBelongToAnotherUser(
                    It.Is<ClearHistoryCommandModel>(x => x == model),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
    
    public static Mock<ICalculationRepository> VerifyAbsentCalculationsWasCalledOnce(
        this Mock<ICalculationRepository> repository,
        ClearHistoryCommandModel model)
    {
        repository.Verify(p =>
                p.CalculationsBelongToAnotherUser(
                    It.Is<ClearHistoryCommandModel>(x => x == model),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
}