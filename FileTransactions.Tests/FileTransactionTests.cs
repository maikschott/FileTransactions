using System;
using System.IO;
using System.Text;
using System.Transactions;
using Xunit;

namespace Masch.FileTransactions.Tests
{
  public class FileTransactionTests : IDisposable
  {
    const string Content = "Test";
    private readonly string source, dest;

    public FileTransactionTests()
    {
      source = Path.Combine(Path.GetTempPath(), "source.txt");
      dest = Path.Combine(Path.GetTempPath(), "dest.txt");
      File.WriteAllText(source, Content);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Copy_Commit(bool hasTransaction)
    {
      Commit(() => FileTransaction.Copy(source, dest), hasTransaction);

      Assert.Equal(Content, File.ReadAllText(source));
      Assert.Equal(Content, File.ReadAllText(dest));
    }

    [Fact]
    public void Copy_Rollback()
    {
      Rollback(() => FileTransaction.Copy(source, dest));

      Assert.Equal(Content, File.ReadAllText(source));
      Assert.False(File.Exists(dest));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Delete_Commit(bool hasTransaction)
    {
      Commit(() => FileTransaction.Delete(source), hasTransaction);

      Assert.False(File.Exists(source));
    }

    [Fact]
    public void Delete_Rollback()
    {
      Rollback(() => FileTransaction.Delete(source));

      Assert.True(File.Exists(source));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Move_Commit(bool hasTransaction)
    {
      Commit(() => FileTransaction.Move(source, dest), hasTransaction);

      Assert.False(File.Exists(source));
      Assert.True(File.Exists(dest));
    }

    [Fact]
    public void Move_Rollback()
    {
      Rollback(() => FileTransaction.Move(source, dest));

      Assert.True(File.Exists(source));
      Assert.False(File.Exists(dest));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Stream_Commit(bool hasTransaction)
    {
      using (var stream = File.Open(source, FileMode.OpenOrCreate))
      {
        // ReSharper disable once AccessToDisposedClosure
        Commit(() => FileTransaction.Stream(stream, fileStream =>
        {
          fileStream.Seek(0, SeekOrigin.End);
          var bytes = Encoding.UTF8.GetBytes("Append");
          fileStream.Write(bytes, 0, bytes.Length);
        }), hasTransaction);
      }

      Assert.Equal(Content + "Append", File.ReadAllText(source));
    }

    [Fact]
    public void Stream_Rollback()
    {
      using (var stream = File.Open(source, FileMode.OpenOrCreate))
      {
        // ReSharper disable once AccessToDisposedClosure
        Rollback(() => FileTransaction.Stream(stream, fileStream =>
        {
          fileStream.Seek(0, SeekOrigin.End);
          var bytes = Encoding.UTF8.GetBytes("Append");
          fileStream.Write(bytes, 0, bytes.Length);
        }));
      }

      Assert.Equal(Content, File.ReadAllText(source));
    }

    public void Dispose()
    {
      if (source != null) { File.Delete(source); }

      if (dest != null) { File.Delete(dest); }
    }

    private static void Commit(Action action, bool hasTransaction)
    {
      if (!hasTransaction)
      {
        action();
        return;
      }

      using var scope = new TransactionScope();
      action();
      scope.Complete();
    }

    private static void Rollback(Action action)
    {
      Assert.Throws<TransactionAbortedException>(() =>
      {
        using var scope = new TransactionScope();
        action();
        Transaction.Current.EnlistVolatile(new RollbackOperation(), EnlistmentOptions.None);
        scope.Complete();
      });
    }

    private class RollbackOperation : IEnlistmentNotification
    {
      public void Prepare(PreparingEnlistment preparingEnlistment) => preparingEnlistment.ForceRollback();

      public void Commit(Enlistment enlistment) => throw new NotSupportedException();

      public void InDoubt(Enlistment enlistment) => throw new NotSupportedException();

      public void Rollback(Enlistment enlistment) => throw new NotSupportedException();
    }
  }
}
