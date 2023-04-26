using System;

/// <summary>
/// Interface for secondary data (abstract and spatial datasets)
/// </summary>
public interface ISecondaryData
{
    //###### Abstract Data ######

    // String identifier for abstract data which is placed in the first row of the csv file
    public const string AbstractDataIdentifier = "AbstractData";

    //###### Spatial Data ######

    // String identifier for abstract data which is placed in the first row of the csv file
    public const string SpatialDataIdentifier = "SpatialData";

    //###### Secondary Data ######

    public enum SecondaryDataType
    {
        Abstract,
        Spatial,
        Unknown,
        NumberOfSecondaryDataTypes
    }
}
