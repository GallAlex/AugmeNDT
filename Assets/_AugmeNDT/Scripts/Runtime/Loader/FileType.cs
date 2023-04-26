
/// <summary>
/// Abstract class for various file types and their parameters
/// </summary>
public abstract class FileType
{
    public FileExtension extensionType = FileExtension.Unknown;
    private string filePath = "";

    #region Getter/Setter
    public string FilePath { get => filePath; set => filePath = value; }
    #endregion

    public override string ToString()
    {
        string values = "Dataset [" + extensionType.ToString() + "] at path: " + FilePath + "\n";

        return base.ToString() + ": \n" + values;
    }
}
