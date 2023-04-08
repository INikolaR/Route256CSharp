namespace Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;

public class OneOrManyCalculationsBelongsToAnotherUserException : Exception
{
    public long[] WrongCalculationIds { get; init; }
    public OneOrManyCalculationsBelongsToAnotherUserException(long[] wrongCalculationIds) : base()
    {
        WrongCalculationIds = (long[])wrongCalculationIds.Clone();
    }
}