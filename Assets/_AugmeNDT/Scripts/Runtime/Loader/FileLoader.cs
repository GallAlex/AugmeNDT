using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
#if !UNITY_EDITOR && UNITY_WSA_10_0
using Windows.Storage;
#endif


namespace AugmeNDT
{
    public enum FileExtension
    {
        Raw,
        Mhd,
        Csv,
        DICOM,
        Unknown,
        NumberOfFileExtensions
    }

    /// <summary>
    /// Abstract class for loader of various files
    /// </summary>
    public abstract class FileLoader
    {
        public string filePath = "";
        public string fileName = "";

        // Defines which type of dataset will be created based on the loaded file
        public FileLoadingManager.DatasetType datasetType = FileLoadingManager.DatasetType.Unknown;

        // Defines which subtype type the secondary dataset has
        public ISecondaryData.SecondaryDataType secondaryDataType = ISecondaryData.SecondaryDataType.Unknown;

        /*Voxel data*/
        public VoxelDataset voxelDataset;

        /*spatial csv data*/
        public PolyFiberData polyFiberDataset;

        /*abstract csv data*/
        public AbstractDataset abstractDataset;


        public abstract Task LoadData(string filePath);

        //public abstract void CreateDataset();

        public override string ToString()
        {
            return base.ToString() + ": \n";
        }


#if UNITY_EDITOR 
    protected static async Task<StreamReader> GetStreamReader(string filePath)
    {
        Stream stream = File.OpenRead(filePath);
        StreamReader reader = new StreamReader(filePath, Encoding.GetEncoding(DetectFileEncoding(stream)));

        return reader;
    }

    protected static async Task<BinaryReader> GetBinaryReader(string filePath)
    {
        BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open));
        return reader;
    }

    protected static async Task<bool> CheckIfFileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    protected static string DetectFileEncoding(Stream fileStream)
    {
        var Utf8EncodingVerifier = Encoding.GetEncoding("utf-8", new EncoderExceptionFallback(), new DecoderExceptionFallback());
        using (var reader = new StreamReader(fileStream, Utf8EncodingVerifier,
                   detectEncodingFromByteOrderMarks: true, leaveOpen: true, bufferSize: 1024))
        {
            string detectedEncoding;
            try
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                }
                detectedEncoding = reader.CurrentEncoding.BodyName;
            }
            catch (Exception e)
            {
                // Failed to decode the file using the BOM/UT8. 
                // Assume it's local ANSI
                detectedEncoding = "ISO-8859-1";
            }
            // Rewind the stream
            fileStream.Seek(0, SeekOrigin.Begin);
            return detectedEncoding;
        }
    }
#endif

#if  !UNITY_EDITOR && (UNITY_ANDROID || MAGIC_LEAP_2)
        protected static async Task<StreamReader> GetStreamReader(string filePath)
        {
            Debug.Log("Opening file: " + filePath);
            try
            {
                Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(DetectFileEncoding(stream)));
                return reader;
            }
            catch (Exception e)
            {
                Debug.LogError("Error opening file: " + e.Message);
                return null;
            }

        }

        protected static async Task<BinaryReader> GetBinaryReader(string filePath)
        {
            Debug.Log("Opening Binary file: " + filePath);
            try
            {
                Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(stream);
                return reader;
            }
            catch (Exception e)
            {
                Debug.LogError("Error opening file: " + e.Message);
                return null;
            }
        }

        protected static async Task<bool> CheckIfFileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        protected static string DetectFileEncoding(Stream fileStream)
        {
            var Utf8EncodingVerifier = Encoding.GetEncoding("utf-8", new EncoderExceptionFallback(), new DecoderExceptionFallback());
            using (var reader = new StreamReader(fileStream, Utf8EncodingVerifier,
                       detectEncodingFromByteOrderMarks: true, leaveOpen: true, bufferSize: 1024))
            {
                string detectedEncoding;
                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                    }
                    detectedEncoding = reader.CurrentEncoding.BodyName;
                }
                catch (Exception e)
                {
                    // Failed to decode the file using the BOM/UT8. 
                    // Assume it's local ANSI
                    detectedEncoding = "ISO-8859-1";
                }
                // Rewind the stream
                fileStream.Seek(0, SeekOrigin.Begin);
                return detectedEncoding;
            }
        }
#endif

#if !UNITY_EDITOR && UNITY_WSA_10_0

        protected static async Task<StreamReader> GetStreamReader(string filePath)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            if (file == null) Debug.LogError("StorageFile is null");

            var randomAccessStream = await file.OpenReadAsync();
            Stream stream = randomAccessStream.AsStreamForRead();

            StreamReader str = new StreamReader(stream, Encoding.GetEncoding(DetectFileEncoding(stream)));

            return str;
        }

        protected static async Task<BinaryReader> GetBinaryReader(string filePath)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            if(file == null) Debug.LogError("StorageFile is null");

            var randomAccessStream = await file.OpenReadAsync();
            Stream stream = randomAccessStream.AsStreamForRead();
            if (stream == null) Debug.LogError("stream is null");
            BinaryReader binr = new BinaryReader(stream);

            return binr;
        }

        protected static async Task<bool> CheckIfFileExists(string filePath)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            }
            catch (Exception)
            {

                return false;
            }

            return true;        
        }

        protected static string DetectFileEncoding(Stream fileStream)
        {
            var Utf8EncodingVerifier = Encoding.GetEncoding("utf-8", new EncoderExceptionFallback(), new DecoderExceptionFallback());
            using (var reader = new StreamReader(fileStream, Utf8EncodingVerifier,
                       detectEncodingFromByteOrderMarks: true, leaveOpen: true, bufferSize: 1024))
            {
                string detectedEncoding;
                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                    }
                    detectedEncoding = reader.CurrentEncoding.BodyName;
                }
                catch (Exception e)
                {
                    // Failed to decode the file using the BOM/UT8. 
                    // Assume it's local ANSI
                    detectedEncoding = "ISO-8859-1";
                }
                // Rewind the stream
                fileStream.Seek(0, SeekOrigin.Begin);
                return detectedEncoding;
            }
        }
#endif
    }
}
