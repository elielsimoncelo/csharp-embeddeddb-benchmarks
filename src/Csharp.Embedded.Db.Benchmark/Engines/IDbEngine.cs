using Csharp.Embedded.Db.Benchmark.Models;
using System.Text;

namespace Csharp.Embedded.Db.Benchmark.Engines
{
    internal interface IDbEngine<TModel>
        where TModel : IModel
    {
        int RecordCount { get; }
        int? ThreadCount { get; }
        StringBuilder Log { get; }
        IDbEngine<TModel> LoadOne();
        IDbEngine<TModel> LoadMany();
        IDbEngine<TModel> RetrieveOne();
        IDbEngine<TModel> RetrieveMany();
        void Start(string context, int recordCount, int? threadCount = null);
    }
}