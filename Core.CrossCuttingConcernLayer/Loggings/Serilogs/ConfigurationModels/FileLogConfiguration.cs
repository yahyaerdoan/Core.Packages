namespace Core.CrossCuttingConcernLayer.Loggings.Serilogs.ConfigurationModels;

public class FileLogConfiguration
{
    public string FolderPath { get; set; }

    /// <summary>Size at which the day's file rolls over to a new one; logging never stops at the limit.</summary>
    public long FileSizeLimitBytes { get; set; } = 5_000_000;

    /// <summary>How many log files to keep; older ones are deleted.</summary>
    public int RetainedFileCountLimit { get; set; } = 31;

    public FileLogConfiguration()
    {
        FolderPath = string.Empty;
    }

    public FileLogConfiguration(string folderPath)
    {
        FolderPath = folderPath;
    }
}
