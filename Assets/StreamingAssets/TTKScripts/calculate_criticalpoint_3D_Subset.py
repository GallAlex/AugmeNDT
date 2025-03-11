import sys
import argparse
import gc
from paraview.simple import *
import pandas as pd
import os

# Parse command-line arguments
parser = argparse.ArgumentParser(description="Process MHD file with subset extraction and save critical points as CSV.")
parser.add_argument("--mhd", type=str, required=True, help="Path to the .mhd file")
parser.add_argument("--csv", type=str, required=True, help="Path to save the output CSV file")
parser.add_argument("--voi", nargs=6, type=int, required=True, 
                    help="Volume of Interest (VOI) indices: imin imax jmin jmax kmin kmax")
args = parser.parse_args()

# Ensure the MHD file exists
print(f"Checking if file exists: {args.mhd}")
if not os.path.exists(args.mhd):
    print(f"ERROR: The file {args.mhd} does not exist!")
    exit(1)

# Ensure a clean ParaView session
Disconnect()
Connect()

# Load the specified MHD file
print(f"Loading MHD file: {args.mhd}")
mhd_data = OpenDataFile(args.mhd)
mhd_data.UpdatePipeline()

# Try different filter names for Extract Subset based on ParaView version
try:
    print("Applying Extract Subset filter...")
    # Try first with ExtractSubset (newer versions)
    try:
        extract_subset = ExtractSubset(Input=mhd_data)
    except NameError:
        try:
            # Try with ExtractGrid (older versions)
            extract_subset = ExtractGrid(Input=mhd_data)
        except NameError:
            # Try with Extract Subset (very old versions or different naming)
            extract_subset = Slice(Input=mhd_data)
            print("Warning: Using Slice filter as fallback for Extract Subset")
    
    # Set VOI (Volume Of Interest)
    voi_values = args.voi
    try:
        extract_subset.VOI = voi_values
    except AttributeError:
        # If it's a Slice filter, we need to set it up differently
        if isinstance(extract_subset, Slice):
            extract_subset.SliceType = "Plane"
            extract_subset.SliceType.Origin = [voi_values[0], voi_values[2], voi_values[4]]
            extract_subset.SliceType.Normal = [0, 0, 1]  # Z-normal slice
            print("Using Slice filter with Z-normal as fallback")
        else:
            raise
    
    print(f"Using VOI: {voi_values}")
    
    # Set Sample Rate as fixed [1, 1, 1] if applicable
    try:
        extract_subset.SampleRate = [1, 1, 1]
        print("Using Sample Rate: [1, 1, 1]")
    except AttributeError:
        print("Sample Rate not applicable for this filter type")
    
    # Include boundary if applicable
    try:
        extract_subset.IncludeBoundary = 1
    except AttributeError:
        pass
    
    # Update the pipeline to apply the subset extraction
    extract_subset.UpdatePipeline()
    print("Extract Subset filter applied successfully.")
    
except Exception as e:
    print(f"ERROR: Extract Subset filter failed with exception: {e}")
    exit(1)

# Apply TTK Scalar Field Critical Points filter to the extracted subset
try:
    print("Applying Critical Points Filter to extracted subset...")
    criticalPoints = TTKScalarFieldCriticalPoints(Input=extract_subset)
    criticalPoints.ScalarField = "MetaImage"
    criticalPoints.Withboundarymask = 1
    criticalPoints.Withvertexidentifiers = 1
    criticalPoints.Withvertexscalars = 1
    criticalPoints.UpdatePipeline()
    print("Critical Points filter applied successfully.")
except Exception as e:
    print(f"ERROR: Critical Points filter failed with exception: {e}")
    exit(1)

# Fetch the output data
print("Fetching Critical Points Data...")
data = servermanager.Fetch(criticalPoints)

# Convert to Pandas DataFrame
print("Converting to DataFrame...")
points = []
for i in range(data.GetNumberOfPoints()):
    x, y, z = data.GetPoint(i)
    critical_type = data.GetPointData().GetArray("CriticalType").GetValue(i)
    is_on_boundary = data.GetPointData().GetArray("IsOnBoundary").GetValue(i)
    
    # MetaImage değeri alınmıyor (dropping magnitude)
    points.append([x, y, z, critical_type, is_on_boundary])

df = pd.DataFrame(points, columns=["X", "Y", "Z", "CriticalType", "IsOnBoundary"])

# Remove rows where IsOnBoundary == 1
df = df[df["IsOnBoundary"] != 1]

# Drop the IsOnBoundary column
df = df.drop(columns=["IsOnBoundary"], errors="ignore")

# Add Id column (0, 1, 2, 3, ...)
df.insert(0, "Id", range(len(df)))

# Rearrange columns to desired order
df = df[["Id", "CriticalType", "X", "Y", "Z"]]

# Save CSV to specified path
print(f"Saving CSV to: {args.csv}")
df.to_csv(args.csv, index=False)
print("PROCESS_IS_SUCCEED CSV saved successfully!")

# --- Cleanup ---
print("Cleaning up memory...")
Delete(criticalPoints)
Delete(extract_subset)
Delete(mhd_data)
servermanager.ProxyManager().UnRegisterProxies()
gc.collect()

print("Done! Memory cleared.")