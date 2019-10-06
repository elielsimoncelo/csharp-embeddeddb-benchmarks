using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Csharp.Embedded.Db.Benchmark.Extensions;
using Csharp.Embedded.Db.Benchmark.Models;
using LevelDB;

namespace Csharp.Embedded.Db.Benchmark.Engines
{
    internal class LevelDbEngine : IDbEngine<LargeModel>
    {
        private readonly List<Action<DB>> _actions;

        public LevelDbEngine()
        {
            _actions = new List<Action<DB>>();
            Log = new StringBuilder();
        }

        public string Context { get; private set; }

        public int RecordCount { get; private set; }

        public int? ThreadCount { get; private set; }

        public StringBuilder Log { get; }

        public IDbEngine<LargeModel> LoadMany()
        {
            var methodName = "LoadMany";

            _actions.Add((db) =>
            {
                Log.AppendLine($"   [ {methodName} records ]");

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                Parallel.For(0, RecordCount, item =>
                {
                    var model = LargeModel.CreateInstance(item);
                    var key = item.ToBinary();
                    var value = model.Serialize();

                    db.Put(key, value, new WriteOptions());
                });

                stopWatch.Stop();

                Log.AppendFormat("      -> {0}: Total time - {1:hh\\:mm\\:ss\\.ffff}\n", methodName, stopWatch.Elapsed);
            });

            return this;
        }

        public IDbEngine<LargeModel> LoadOne()
        {
            var methodName = "LoadOne";

            _actions.Add((db) =>
            {
                Log.AppendLine($"   [ {methodName} record ]");

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var item = 2000001L;
                var model = LargeModel.CreateInstance(item);
                var key = item.ToBinary();
                var value = model.Serialize();

                db.Put(key, value, new WriteOptions());

                stopWatch.Stop();

                Log.AppendFormat("      -> {0}: Total time - {1:hh\\:mm\\:ss\\.ffff}\n", methodName, stopWatch.Elapsed);
            });

            return this;
        }

        public IDbEngine<LargeModel> RetrieveMany()
        {
            var methodName = "RetrieveMany";

            _actions.Add((db) =>
            {
                Log.AppendLine($"   [ {methodName} records ]");

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                Parallel.For(0, RecordCount, item =>
                {
                    var key = item.ToBinary();
                    var value = db.Get(key, new ReadOptions());
                    var model = value.Deserialize<LargeModel>();

                    if (model == null)
                    {
                        lock (Log) { Log.AppendLine($"           -> Null item {item}"); }
                    }
                });

                stopWatch.Stop();

                Log.AppendFormat("      -> {0}: Total time - {1:hh\\:mm\\:ss\\.ffff}\n", methodName, stopWatch.Elapsed);
            });

            return this;
        }

        public IDbEngine<LargeModel> RetrieveOne()
        {
            var methodName = "RetrieveOne";

            _actions.Add((db) =>
            {
                Log.AppendLine($"   [ {methodName} record ]");

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var key = 2000001L.ToBinary();
                var value = db.Get(key, new ReadOptions());
                var model = value.Deserialize<LargeModel>();

                Log.AppendFormat($"     -> {model.ToString()}\n");

                stopWatch.Stop();

                Log.AppendFormat("      -> {0}: Total time - {1:hh\\:mm\\:ss\\.ffff}\n", methodName, stopWatch.Elapsed);
            });

            return this;
        }

        public void Start(string context, int recordCount, int? threadCount = null)
        {
            Log.Clear();

            Context = context;
            RecordCount = recordCount;
            ThreadCount = threadCount;
            var engine = GetType().Name;

            Log.AppendLine($"# Context: {Context} | Engine: {engine} | Records: {recordCount} | Threads: {threadCount}");

            var databaseName = $@"{context.Replace(" ", "_").ToLower()}_{engine.ToLower()}";

            if (Directory.Exists(databaseName))
                Directory.Delete(databaseName, true);

            var stopWatch = new Stopwatch();
            var db = new DB(new Options { CreateIfMissing = true }, databaseName);

            stopWatch.Start();
            _actions.ForEach(action => action(db));
            stopWatch.Stop();

            db.Close();

            Log.AppendFormat("# Context: {0} | Engine: {1} | Total time: {2:hh\\:mm\\:ss\\.ffff}\n", Context, engine, stopWatch.Elapsed);
            Log.AppendLine("\n");
        }
    }
}