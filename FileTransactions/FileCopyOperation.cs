using System.IO;

namespace Masch.FileTransactions
{
  internal class FileCopyOperation : FileOperationBase
  {
    private readonly string sourceFileName;
    private readonly string destFileName;
    private readonly bool overwrite;

    public FileCopyOperation(string sourceFileName, string destFileName, bool overwrite = false)
    {
      this.sourceFileName = sourceFileName;
      this.destFileName = destFileName;
      this.overwrite = overwrite;
    }

    internal override void Execute()
    {
      File.Copy(sourceFileName, destFileName, overwrite);
    }

    protected override void Commit()
    {
    }

    protected override void Rollback()
    {
      File.Delete(destFileName);
    }
  }
}
