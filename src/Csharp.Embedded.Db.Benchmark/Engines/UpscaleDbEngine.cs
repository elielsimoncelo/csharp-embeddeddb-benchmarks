using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Csharp.Embedded.Db.Benchmark.Extensions;
using Csharp.Embedded.Db.Benchmark.Models;
using Upscaledb;

namespace Csharp.Embedded.Db.Benchmark.Engines
{
    internal class UpscaleDbEngine : IDbEngine<LargeModel>
    { 
        private readonly List<Action<Database>> _actions;

        public UpscaleDbEngine()
        {
            _actions = new List<Action<Database>>();
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

                    db.Insert(key, value);
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

                db.Insert(key, value);

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
                    var value = db.Find(key);
                    var model = value.Deserialize<LargeModel>();

                    if (model == null)
                    {
                        lock (Log) { Log.AppendLine($"      -> Null item {item}"); }
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
                var value = db.Find(key);
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

            var databaseName = $@"{context.Replace(" ", "_").ToLower()}_{engine.ToLower()}.db";

            if (Directory.Exists(databaseName))
                Directory.Delete(databaseName, true);

            var databasePath = Path.Combine(System.Environment.CurrentDirectory, databaseName);

            var environment = new Upscaledb.Environment();
            var db = new Database();

            environment.Create(databaseName);
            db = environment.CreateDatabase(1);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            _actions.ForEach(action => action(db));

            stopWatch.Stop();

            environment.Flush();
            environment.Close();
            db.Close();

            Log.AppendFormat("# Context: {0} | Engine: {1} | Total time: {2:hh\\:mm\\:ss\\.ffff}\n", Context, engine, stopWatch.Elapsed);
            Log.AppendLine("\n");
        }
    }
}