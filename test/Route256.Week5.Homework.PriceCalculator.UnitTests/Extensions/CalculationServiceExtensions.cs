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
            p.ClearHistory(It.IsAny<ClearHistoryCommand>(), 
                It.IsAny<CancellationToken>()));

        return repository;
    }
    
    public static Mock<ICalculationService> SetupCalculationsBelongToAnotherUser(
        this Mock<ICalculationService> repository,
        long[] ids)
    {
        repository.Setup(p =>
                p.CalculationsBelongToAnotherUser(It.IsAny<ClearHistoryCommand>(), 
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);

        return repository;
    }
    
    public static Mock<ICalculationService> SetupCalculationsBelongToAnotherUserThrows(
        this Mock<ICalculationService> repository,
        long[] ids)
    {
        repository.Setup(p =>
                p.CalculationsBelongToAnotherUser(It.IsAny<ClearHistoryCommand>(), 
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OneOrManyCalculationsBelongsToAnotherUserException(Array.Empty<long>()));

        return repository;
    }
    
    public static Mock<ICalculationService> SetupAbsentCalculations(
        this Mock<ICalculationService> repository,
        long[] ids)
    {
        repository.Setup(p =>
                p.AbsentCalculations(It.IsAny<ClearHistoryCommand>(), 
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);

        return repository;
    }
    
    public static Mock<ICalculationService> SetupAbsentCalculationsThrows(
        this Mock<ICalculationService> repository,
        long[] ids)
    {
        repository.Setup(p =>
                p.AbsentCalculations(It.IsAny<ClearHistoryCommand>(), 
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
        ClearHistoryCommand command)
    {
        repository.Verify(p =>
                p.ClearHistory(
                    It.Is<ClearHistoryCommand>(x => x == command),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
    
    public static Mock<ICalculationService> VerifyCalculationsBelongToAnotherUserWasCalledOnce(
        this Mock<ICalculationService> repository,
        ClearHistoryCommand model)
    {
        repository.Verify(p =>
                p.CalculationsBelongToAnotherUser(
                    It.Is<ClearHistoryCommand>(x => x == model),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
    
    public static Mock<ICalculationService> VerifyAbsentCalculationsWasCalledOnce(
        this Mock<ICalculationService> repository,
        ClearHistoryCommand model)
    {
        repository.Verify(p =>
                p.CalculationsBelongToAnotherUser(
                    It.Is<ClearHistoryCommand>(x => x == model),
                    It.IsAny<CancellationToken>()),
            Times.Once);
        
        return repository;
    }
}