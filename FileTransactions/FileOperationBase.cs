using System;
using System.Transactions;

namespace Masch.FileTransactions
{
  internal abstract class FileOperationBase : IEnlistmentNotification
  {
    public void Prepare(PreparingEnlistment preparingEnlistment)
    {
      try
      {
        Prepare();
        Execute();
        preparingEnlistment.Prepared();
      }
      catch (Exception e)
      {
        preparingEnlistment.ForceRollback(e);
      }
    }

    public void Commit(Enlistment enlistment)
    {
      Commit();
      enlistment.Done();
    }

    public virtual void InDoubt(Enlistment enlistment)
    {
      Rollback(enlistment);
    }

    public void Rollback(Enlistment enlistment)
    {
      Rollback();
      enlistment.Done();
    }

    internal abstract void Execute();

    protected virtual void Prepare() { }

    protected abstract void Commit();

    protected abstract void Rollback();
  }
}