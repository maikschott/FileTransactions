# Transactional file operations

Implementation of transactional file operations in C# for copying files, deleting files, moving files, and file stream operations, 
which rollback when an exception occurs.

This uses the transaction from the [System.Transaction](https://docs.microsoft.com/dotnet/api/system.transactions) namespace.
This class can be used in conjunction with all kinds of classes supporting TransactionScopes, like [SqlConnection](https://docs.microsoft.com/dotnet/api/system.data.sqlclient.sqlconnection),
which allows the rollback of file and database operations.

## Operations
The operations are:
* `FileTransaction.Copy(string sourceFileName, string destFileName, bool overwrite = false)`
* `FileTransaction.Move(string sourceFileName, string destFileName)`
* `FileTransaction.Delete(string fileName)`
* `FileTransaction.Stream(FileStream fileStream, Action<FileStream> action)`

## Usage
```csharp
using (var scope = new TransactionScope())
{
  FileTransaction.Copy(source, dest);
  scope.Complete();
}
```