using System.IO;

namespace Masch.FileTransactions
{
  internal class FileMoveOperation : FileOperationBase
  {
    private readonly string destPath;
    private readonly string sourcePath;

    public FileMoveOperation(string sourcePath, string destPath)
    {
      this.sourcePath = sourcePath;
      this.destPath = destPath;
    }

    internal override void Execute()
    {
      File.Move(sourcePath, destPath);
    }

    protected override void Commit()
    {
    }

    protected override void Rollback()
    {
      File.Move(destPath, sourcePath);
    }
  }
}