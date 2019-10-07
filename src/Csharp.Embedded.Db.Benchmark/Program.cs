using System;
using System.Collections.Generic;
using Csharp.Embedded.Db.Benchmark.Engines;
using Csharp.Embedded.Db.Benchmark.Models;

namespace Csharp.Embedded.Db.Benchmark
{
    internal class Program
    {
        private static readonly List<(string Context, int RecordCount, int? ThreadCount)> _configurations = new List<(string Context, int RecordCount, int? ThreadCount)>
        {
            ("Parallel sample", 1000000, null)
        };

        private static readonly List<IDbEngine<LargeModel>> _dbEngines = new List<IDbEngine<LargeModel>>
        {
            new UpscaleDbEngine(),
            new LevelDbEngine(),
            new LiteDbEngine()
        };

        private static void Main()
        {
            foreach (var configuration in _configurations)
            {
                foreach (var dbEngine in _dbEngines)
                {
                    dbEngine.LoadMany()
                            .LoadOne()
                            .RetrieveMany()
                            .RetrieveOne()
                            .Start(configuration.Context, configuration.RecordCount, configuration.ThreadCount);

                    Console.WriteLine(dbEngine.Log);
                }
            }

            Console.WriteLine("Finalized...");
            Console.ReadKey();
        }
    }
}