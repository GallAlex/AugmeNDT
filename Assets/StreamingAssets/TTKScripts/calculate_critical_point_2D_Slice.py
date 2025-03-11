import sys
import argparse
import gc
from paraview.simple import *
import pandas as pd
import os

# Parse command-line arguments
parser = argparse.ArgumentParser(description="Process MHD file and save critical points as CSV.")
parser.add_argument("--mhd", type=str, required=True, help="Path to the .mhd file")
parser.add_argument("--voi", nargs=6, type=int, required=True, 
                    help="Volume of Interest (VOI) indices: imin imax jmin jmax kmin kmax")
parser.add_argument("--origin", nargs=3, type=float, required=True, help="Origin coordinates (x, y, z)")
parser.add_argument("--normal", nargs=3, type=float, required=True, help="Normal vector (x, y, z)")
parser.add_argument("--csv", type=str, required=True, help="Path to save the output CSV file")
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

# Apply Extract Subset filter
print("Applying Extract Subset Filter...")
extract_subset = ExtractSubset(Input=mhd_data)
extract_subset.VOI = args.voi  # Use the VOI arguments: [imin, imax, jmin, jmax, kmin, kmax]
extract_subset.UpdatePipeline()

# Apply Slice (Kesit) filter
print("Applying Slice Filter...")
sliceFilter = Slice(Input=extract_subset)  # Changed input from mhd_data to extract_subset
sliceFilter.SliceType = "Plane"
sliceFilter.SliceType.Origin = args.origin
sliceFilter.SliceType.Normal = args.normal
sliceFilter.UpdatePipeline()

# Apply TTK Scalar Field Critical Points filter
try:
    print("Applying Critical Points Filter...")
    criticalPoints = TTKScalarFieldCriticalPoints(Input=sliceFilter)
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
    meta_image_value = data.GetPointData().GetArray("MetaImage").GetValue(i)
    
    points.append([x, y, z, critical_type, is_on_boundary, meta_image_value])

df = pd.DataFrame(points, columns=["X", "Y", "Z", "CriticalType", "IsOnBoundary", "MetaImage"])

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
print(" PROCESS_IS_SUCCEED CSV saved successfully!")

# --- Cleanup ---
print("Cleaning up memory...")
Delete(criticalPoints)
Delete(sliceFilter)
Delete(extract_subset)  # Added cleanup for extract_subset
Delete(mhd_data)
servermanager.ProxyManager().UnRegisterProxies()
gc.collect()

print("Done! Memory cleared.")