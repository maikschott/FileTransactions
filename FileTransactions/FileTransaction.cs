using System;
using System.IO;
using System.Transactions;

namespace Masch.FileTransactions
{
  public static class FileTransaction
  {
    public static void Copy(string sourceFileName, string destFileName, bool overwrite = false) => Enlist(new FileCopyOperation(sourceFileName, destFileName, overwrite));

    public static void Move(string sourceFileName, string destFileName) => Enlist(new FileMoveOperation(sourceFileName, destFileName));

    public static void Delete(string fileName) => Enlist(new FileDeleteOperation(fileName));

    public static void Stream(FileStream fileStream, Action<FileStream> action) => Enlist(new FileStreamOperation(fileStream, action));

    private static void Enlist(FileOperationBase enlistment)
    {
      var transaction = Transaction.Current;
      if (transaction == null) { enlistment.Execute(); }
      else { transaction.EnlistVolatile(enlistment, EnlistmentOptions.None); }
    }
  }
}