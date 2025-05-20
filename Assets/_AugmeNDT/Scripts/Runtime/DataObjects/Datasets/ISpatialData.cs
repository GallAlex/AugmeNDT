// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

namespace AugmeNDT{
    public interface ISpatialData : ISecondaryData
    {
        public enum SpatialDatasetType
        {
            Fiber,
            Pore,
            NumberOfSpatialDatasetTypes
        }

        // Defines how many rows to skip at the beginning of the csv file
        public const int SkipRows = 4;

        // String identifier for fiber data which is placed after the SpatialDataIdentifier with an comma
        public const string FiberIdentifier = "Fiber";

        // String identifier for pore data which is placed after the SpatialDataIdentifier with an comma
        public const string PoreIdentifier = "Pore";

    }
}