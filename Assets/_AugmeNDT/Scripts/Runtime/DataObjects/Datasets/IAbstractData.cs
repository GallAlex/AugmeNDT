using System;

public interface IAbstractData : ISecondaryData
{
    // Defines how many rows to skip at the beginning of the csv file
    public const int SkipRows = 0;
}
