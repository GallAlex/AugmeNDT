import argparse
import gc
from paraview.simple import *
import pandas as pd
import os
import sys

# Argument parser for command-line input
parser = argparse.ArgumentParser(description="Set mhd file, origin, normal, and CSV output path for slice filter.")
parser.add_argument("--mhd", type=str, required=True, help="Path to the .mhd file")
parser.add_argument("--voi", nargs=6, type=int, required=True, 
                    help="Volume of Interest (VOI) indices: imin imax jmin jmax kmin kmax")
parser.add_argument("--origin", nargs=3, type=float, required=True, help="Origin coordinates (x, y, z)")
parser.add_argument("--normal", nargs=3, type=float, required=True, help="Normal vector (x, y, z)")
parser.add_argument("--csv", type=str, required=True, help="Path to save the output CSV file")
args = parser.parse_args()

print(f" Checking if file exists: {args.mhd}")
if not os.path.exists(args.mhd):
    print(f" ERROR: The file {args.mhd} does not exist!")
    exit(1)

# Ensure a clean ParaView session
Disconnect()
Connect()

print(" File exists, loading MHD file...")
mhd_data = OpenDataFile(args.mhd)
if mhd_data is None:
    print(f" ERROR: Failed to open MHD file: {args.mhd}")
    exit(1)

mhd_data.UpdatePipeline()

# Apply Extract Subset filter
print(" Applying Extract Subset Filter...")
extract_subset = ExtractSubset(Input=mhd_data)
extract_subset.VOI = args.voi  # Use the VOI arguments: [imin, imax, jmin, jmax, kmin, kmax]
extract_subset.UpdatePipeline()

print(" Creating Slice Filter...")
sliceFilter = Slice(Input=extract_subset)  # Changed input from mhd_data to extract_subset
sliceFilter.SliceType = "Plane"
sliceFilter.SliceType.Origin = args.origin
sliceFilter.SliceType.Normal = args.normal
sliceFilter.UpdatePipeline()

# Slice filter sonrası veri kontrolü
if sliceFilter.GetPointDataInformation().GetNumberOfArrays() == 0:
    print(" ERROR: Slice filter produced no data. Check slice parameters.")
    exit(1)

print(" Checking available arrays in slice output:")
available_arrays = [sliceFilter.PointData[i].Name for i in range(sliceFilter.PointData.GetNumberOfArrays())]
print(f" Available arrays: {available_arrays}")

print(" Applying Gradient Filter...")
gradientFilter = Gradient(Input=sliceFilter)

# Validate if "MetaImage" exists
if "MetaImage" not in available_arrays:
    print(f" ERROR: 'MetaImage' scalar array not found! Available arrays: {available_arrays}")
    exit(1)

gradientFilter.ScalarArray = ["POINTS", "MetaImage"]

# Apply pipeline update
print(" Updating gradient filter pipeline...")
gradientFilter.UpdatePipeline()  # Bu satır hata verirse direkt patlayacak
print(" Gradient filter applied successfully.")

print(" Fetching Data from Server...")
data = servermanager.Fetch(gradientFilter)
if data.GetNumberOfPoints() == 0:
    print(" ERROR: Gradient filter produced no points. Check input data or parameters.")
    exit(1)
    
print(f" Number of points in result: {data.GetNumberOfPoints()}")
print(f" Number of arrays in result: {data.GetPointData().GetNumberOfArrays()}")

print(" Extracting Points...")
points = []
array_names = [data.GetPointData().GetArrayName(i) for i in range(data.GetPointData().GetNumberOfArrays())]
print(f" Result arrays: {array_names}")

for i in range(data.GetNumberOfPoints()):
    row = list(data.GetPoint(i))  # Get X, Y, Z coordinates
    for array_name in array_names:
        array = data.GetPointData().GetArray(array_name)
        if array is not None:
            components = [array.GetComponent(i, j) for j in range(array.GetNumberOfComponents())]
            row.append(components)
    points.append(row)

# Create Pandas DataFrame
print(" Creating DataFrame...")
column_names = ["X", "Y", "Z"] + array_names
df = pd.DataFrame(points, columns=column_names)

# Check DataFrame content
print(f" DataFrame shape: {df.shape}")
if df.empty:
    print(" ERROR: DataFrame is empty. No data was extracted.")
    exit(1)

# Drop "MetaImage" column if it exists
df = df.drop(columns=["MetaImage"], errors="ignore")

# Split "Gradient" column into X_Mag, Y_Mag, Z_Mag if it exists
if "Gradient" in df.columns:
    df[["X_Mag", "Y_Mag", "Z_Mag"]] = df["Gradient"].apply(pd.Series)
    
    # Calculate magnitude of the gradient vector
    df["Magnitude"] = (df["X_Mag"]**2 + df["Y_Mag"]**2 + df["Z_Mag"]**2).apply(lambda x: x**0.5)
    
    df = df.drop(columns=["Gradient"], errors="ignore")

# Save CSV output
print(f" Saving results to CSV: {args.csv}")
df.to_csv(args.csv, index=False)
print(f" CSV saved successfully with {len(df)} rows")
print(f" PROCESS_IS_SUCCEED CSV saved at: {args.csv}")

# --- Cleanup ---
print(" Cleaning up memory...")
# Try to cleanup but don't catch errors - let them appear
Delete(gradientFilter)
Delete(sliceFilter)
Delete(extract_subset)  # Added cleanup for extract_subset
Delete(mhd_data)
servermanager.ProxyManager().UnRegisterProxies()
gc.collect()

print(" Done! Memory cleared.")
print(" Script completed.")