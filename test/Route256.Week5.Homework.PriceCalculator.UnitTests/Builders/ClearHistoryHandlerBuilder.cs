using Moq;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Stubs;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;

public class ClearHistoryHandlerBuilder
{
    public Mock<ICalculationService> CalculationService;
    
    public ClearHistoryHandlerBuilder()
    {
        CalculationService = new Mock<ICalculationService>();
    }
    
    public ClearHistoryHandlerStub Build()
    {
        return new ClearHistoryHandlerStub(
            CalculationService);
    }
}