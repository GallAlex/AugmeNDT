import sys
import argparse
import gc
from paraview.simple import *
import pandas as pd
import os

# Parse command-line arguments
parser = argparse.ArgumentParser(description="Process MHD file and save critical points as CSV.")
parser.add_argument("--mhd", type=str, required=True, help="Path to the .mhd file")
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

# Apply TTK Scalar Field Critical Points filter directly to the 3D object
try:
    print("Applying Critical Points Filter to entire object...")
    criticalPoints = TTKScalarFieldCriticalPoints(Input=mhd_data)
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
print(" PROCESS_IS_SUCCEED CSV saved successfully!")

# --- Cleanup ---
print("Cleaning up memory...")
Delete(criticalPoints)
Delete(mhd_data)
servermanager.ProxyManager().UnRegisterProxies()
gc.collect()

print("Done! Memory cleared.")