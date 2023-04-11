using System;
using System.Linq;
using System.Threading;
using Moq;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Comparers;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Extensions;

public static class CalculationServiceExtensions
{
    public static Mock<ICalculationService> SetupSaveCalculation(
        this Mock<ICalculationService> service,
        long id)
    {
        service.Setup(p =>
                p.SaveCalculation(It.IsAny<SaveCalculationModel>(), 
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(id);

        return service;
    }
    
    public static Mock<ICalculationService> SetupCalculatePriceByVolume(
        this Mock<ICalculationService> service,
        double volume,
        decimal price)
    {
        service.Setup(p =>
                p.CalculatePriceByVolume(It.IsAny<GoodModel[]>(), 
                    out volume))
            .Returns(price);

        return service;
    }
    
    public static Mock<ICalculationService> SetupCalculatePriceByWeight(
        this Mock<ICalculationService> service,
        double weight,
        decimal price)
    {
        service.Setup(p =>
                p.CalculatePriceByWeight(It.IsAny<GoodModel[]>(), 
                    out weight))
            .Returns(price);

        return service;
    }

    public static Mock<ICalculationService> SetupQueryCalculations(
        this Mock<ICalculationService> service,
        QueryCalculationModel[] model)
    {
        service.Setup(p =>
                p.QueryCalculations(It.IsAny<QueryCalculationFilter>(), 
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        return service;
    }
    
    public static Mock<ICalculationService> SetupClearHistory(
        this Mock<ICalculationService> repository)
    {
        repository.Setup(p =>
            p.ClearHistory(It.IsAny<ClearHistoryModel>(), 
                It.IsAny<CancellationToken>()));

        return repository;
    }
    
    public static Mock<ICalculationService> SetupClearAllHistory(
        this Mock<ICalculationService> repository)
    {
        repository.Setup(p =>
            p.ClearAllHistory(It.IsAny<ClearAllHistoryModel>(), 
                It.IsAny<CancellationToken>()));

        return repository;
    }
    
    public static Mock<ICalculationService> SetupConnectedGoodIdsQuery(
        this Mock<ICalculationService> repository,
        long[] connectedGoodIds)
    {
        repository.Setup(p =>
            p.ConnectedGoodIdsQuery(It.IsAny<QueryModel>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectedGoodIds);

        return repository;
    }
    
    public static Mock<ICalculationService> SetupAllConnectedGoodIdsQuery(
        this Mock<ICalculationService> repository,
        long[] connectedGoodIds)
    {
        repository.Setup(p =>
                p.AllConnectedGoodIdsQuery(It.IsAny<long>(), 
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectedGoodIds);

        return repository;
    }
    
    public static Mock<ICalculationService> SetupCalculationsBelongToAnotherUser(
        this Mock<ICalculationService> repository,
        long[] ids)
    {
        repository.Setup(p =>
                p.CalculationsBelongToAnotherUser(It.IsAny<QueryModel>(), 
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);

        return repository;
    }
    
    public static Mock<ICalculationService> SetupCalculationsBelongToAnotherUserThrows(
        this Mock<ICalculationService> repository,
        long[] ids)
    {
        repository.Setup(p =>
                p.CalculationsBelongToAnotherUser(It.IsAny<QueryModel>(), 
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OneOrManyCalculationsBelongsToAnotherUserException(Array.Empty<long>()));

        return repository;
    }
    
    public static Mock<ICalculationService> SetupAbsentCalculations(
        this Mock<ICalculationService> repository,
        long[] ids)
    {
        repository.Setup(p =>
                p.AbsentCalculations(It.IsAny<QueryModel>(), 
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);

        return repository;
    }
    
    public static Mock<ICalculationService> SetupAbsentCalculationsThrows(
        this Mock<ICalculationService> repository,
        long[] ids)
    {
        repository.Setup(p =>
                p.AbsentCalculations(It.IsAny<QueryModel>(), 
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OneOrManyCalculationsNotFoundException());

        return repository;
    }
    
    public static Mock<ICalculationService> VerifySaveCalculationWasCalledOnce(
        this Mock<ICalculationService> service,
        SaveCalculationModel model)
    {
        service.Verify(p =>
                p.SaveCalculation(
                    It.Is<SaveCalculationModel>(x => new CalculationModelComparer().Equals(x, model)),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return service;
    }
    
    public static Mock<ICalculationService> VerifyCalculatePriceByVolumeWasCalledOnce(
        this Mock<ICalculationService> service,
        GoodModel[] model)
    {
        service.Verify(p =>
                p.CalculatePriceByVolume(
                    It.Is<GoodModel[]>(x => x.SequenceEqual(model)),
                    out It.Ref<double>.IsAny),
            Times.Once);
        
        return service;
    }
    
    public static Mock<ICalculationService> VerifyCalculatePriceByWeightWasCalledOnce(
        this Mock<ICalculationService> service,
        GoodModel[] model)
    {
        service.Verify(p =>
                p.CalculatePriceByWeight(
                    It.Is<GoodModel[]>(x => x.SequenceEqual(model)),
                    out It.Ref<double>.IsAny),
            Times.Once);

        return service;
    }
    
    public static Mock<ICalculationService> VerifyQueryCalculationsWasCalledOnce(
        this Mock<ICalculationService> service,
        QueryCalculationFilter filter)
    {
        service.Verify(p =>
                p.QueryCalculations(
                    It.Is<QueryCalculationFilter>(x => x == filter),
                    It.IsAny<CancellationToken>()),
            Times.Once);

        return service;
    }
    
    public static Mock<ICalculationService> VerifyClearHistoryWasCalledOnce(
        this Mock<ICalculationService> repository,
        ClearHistoryModel model)
    {
        repository.Verify(p =>
                p.ClearHistory(
                    It.Is<ClearHistoryModel>(x => x == model),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
    
    public static Mock<ICalculationService> VerifyConnectedGoodIdsQueryWasCalledOnce(
        this Mock<ICalculationService> repository,
        QueryModel model)
    {
        repository.Verify(p =>
                p.ConnectedGoodIdsQuery(
                    It.Is<QueryModel>(x => x == model),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
    public static Mock<ICalculationService> VerifyAllConnectedGoodIdsQueryWasCalledOnce(
        this Mock<ICalculationService> repository,
        long userId)
    {
        repository.Verify(p =>
                p.AllConnectedGoodIdsQuery(
                    It.Is<long>(x => x == userId),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
    
    public static Mock<ICalculationService> VerifyClearAllHistoryWasCalledOnce(
        this Mock<ICalculationService> repository,
        ClearAllHistoryModel model)
    {
        repository.Verify(p =>
                p.ClearAllHistory(
                    It.Is<ClearAllHistoryModel>(x => x == model),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
    
    public static Mock<ICalculationService> VerifyCalculationsBelongToAnotherUserWasCalledOnce(
        this Mock<ICalculationService> repository,
        QueryModel model)
    {
        repository.Verify(p =>
                p.CalculationsBelongToAnotherUser(
                    It.Is<QueryModel>(x => x == model),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
    
    public static Mock<ICalculationService> VerifyAbsentCalculationsWasCalledOnce(
        this Mock<ICalculationService> repository,
        QueryModel model)
    {
        repository.Verify(p =>
                p.CalculationsBelongToAnotherUser(
                    It.Is<QueryModel>(x => x == model),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
}