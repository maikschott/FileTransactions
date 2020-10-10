using System;
using System.IO;

namespace Masch.FileTransactions
{
  internal class FileStreamOperation : FileOperationBase
  {
    private readonly Action<FileStream> action;
    private readonly FileStream fileStream;
    private string? backupFile;
    private long backupPosition;

    public FileStreamOperation(FileStream fileStream, Action<FileStream> action)
    {
      this.fileStream = fileStream;
      this.action = action;

      if (!fileStream.CanWrite)
      {
        return;
      }

      if (!fileStream.CanSeek || !fileStream.CanRead)
      {
        throw new NotSupportedException("Writable FileStreams must support seeks and reads");
      }
    }

    internal override void Execute()
    {
      action(fileStream);
    }

    protected override void Prepare()
    {
      if (!fileStream.CanWrite)
      {
        backupFile = null;
        return;
      }

      backupFile = Path.GetTempFileName();

      backupPosition = fileStream.Position;
      try
      {
        fileStream.Position = 0;
        using (var backupStream = File.OpenWrite(backupFile))
        {
          fileStream.CopyTo(backupStream);
        }
      }
      finally
      {
        fileStream.Position = backupPosition;
      }
    }

    protected override void Commit()
    {
      if (backupFile != null)
      {
        File.Delete(backupFile);
      }
    }

    protected override void Rollback()
    {
      if (backupFile == null) { return; }

      fileStream.Position = 0;
      using (var backupStream = File.OpenRead(backupFile))
      {
        backupStream.CopyTo(fileStream);
      }

      fileStream.SetLength(fileStream.Position);
      fileStream.Position = backupPosition;

      File.Delete(backupFile);
    }
  }
}