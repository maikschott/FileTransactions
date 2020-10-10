using System.IO;

namespace Masch.FileTransactions
{
  internal class FileDeleteOperation : FileOperationBase
  {
    private readonly string path;
    private string? backupPath;

    public FileDeleteOperation(string path)
    {
      this.path = path;
    }

    internal override void Execute()
    {
      File.Delete(path);
    }

    protected override void Prepare()
    {
      if (File.Exists(path))
      {
        backupPath = Path.GetTempFileName();
        File.Copy(path, backupPath, true);
      }
    }

    protected override void Commit()
    {
      if (backupPath != null) 
      {
        File.Delete(backupPath);
      }
    }

    protected override void Rollback()
    {
      if (backupPath != null && File.Exists(backupPath))
      {
        File.Move(backupPath, path);
      }
    }
  }
}