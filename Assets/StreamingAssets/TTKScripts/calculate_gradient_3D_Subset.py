import argparse
import gc
from paraview.simple import *
import pandas as pd
import os

# Argument parser for command-line input
parser = argparse.ArgumentParser(description="Set mhd file, VOI parameters, and CSV output path for gradient filter.")
parser.add_argument("--mhd", type=str, required=True, help="Path to the .mhd file")
parser.add_argument("--csv", type=str, required=True, help="Path to save the output CSV file")
parser.add_argument("--voi", nargs=6, type=int, required=True, 
                    help="Volume of Interest (VOI) indices: imin imax jmin jmax kmin kmax")
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
mhd_data.UpdatePipeline()

# Try different filter names for Extract Subset based on ParaView version
try:
    print(" Applying Extract Subset filter...")
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
            print(" Warning: Using Slice filter as fallback for Extract Subset")
    
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
            print(" Using Slice filter with Z-normal as fallback")
        else:
            raise
    
    print(f" Using VOI: {voi_values}")
    
    # Set Sample Rate as fixed [1, 1, 1] if applicable
    try:
        extract_subset.SampleRate = [1, 1, 1]
        print(" Using Sample Rate: [1, 1, 1]")
    except AttributeError:
        print(" Sample Rate not applicable for this filter type")
    
    # Include boundary if applicable
    try:
        extract_subset.IncludeBoundary = 1
    except AttributeError:
        pass
    
    # Update the pipeline to apply the subset extraction
    extract_subset.UpdatePipeline()
    print(" Extract Subset filter applied successfully.")
    
except Exception as e:
    print(f" ERROR: Extract Subset filter failed with exception: {e}")
    exit(1)

try:
    print(" Applying Gradient Filter to extracted subset...")
    gradientFilter = Gradient(Input=extract_subset)

    # Validate if "MetaImage" exists
    available_arrays = [extract_subset.PointData[i].Name for i in range(extract_subset.PointData.GetNumberOfArrays())]
    if "MetaImage" not in available_arrays:
        print(f" ERROR: 'MetaImage' scalar array not found! Available arrays: {available_arrays}")
        exit(1)

    gradientFilter.ScalarArray = ["POINTS", "MetaImage"]
    
    # Apply pipeline update with error catching
    gradientFilter.UpdatePipeline()
    print(" Gradient filter applied successfully.")

except Exception as e:
    print(f" ERROR: Gradient filter failed with exception: {e}")
    exit(1)


print(" Fetching Data from Server...")
data = servermanager.Fetch(gradientFilter)

print(" Extracting Points...")
points = []
array_names = [data.GetPointData().GetArrayName(i) for i in range(data.GetPointData().GetNumberOfArrays())]

for i in range(data.GetNumberOfPoints()):
    row = list(data.GetPoint(i))  # Get X, Y, Z coordinates
    for array_name in array_names:
        array = data.GetPointData().GetArray(array_name)
        if array is not None:
            row.append([array.GetComponent(i, j) for j in range(array.GetNumberOfComponents())])
    points.append(row)

# Create Pandas DataFrame
print(" Creating DataFrame...")
column_names = ["X", "Y", "Z"] + array_names
df = pd.DataFrame(points, columns=column_names)

# Drop "MetaImage" column if it exists
df = df.drop(columns=["MetaImage"], errors="ignore")

# Split "Gradient" column into X_Mag, Y_Mag, Z_Mag if it exists
if "Gradient" in df.columns:
    df[["X_Mag", "Y_Mag", "Z_Mag"]] = df["Gradient"].apply(pd.Series)
    
    # Calculate magnitude of the gradient vector
    df["Magnitude"] = (df["X_Mag"]**2 + df["Y_Mag"]**2 + df["Z_Mag"]**2).apply(lambda x: x**0.5)
    
    df = df.drop(columns=["Gradient"], errors="ignore")

# Save CSV output
df.to_csv(args.csv, index=False)
print(f" PROCESS_IS_SUCCEED CSV saved at: {args.csv}")

# --- Cleanup ---
print(" Cleaning up memory...")
Delete(gradientFilter)
Delete(extract_subset)
Delete(mhd_data)
servermanager.ProxyManager().UnRegisterProxies()
gc.collect()

print(" Done! Memory cleared.")