using System;
using System.Collections.Generic;
using ZeroFormatter;

namespace Csharp.Embedded.Db.Benchmark.Models
{
    [ZeroFormattable]
    public class LargeModel: IModel
    {
        public LargeModel() { }

        public LargeModel
        (
            long id,
            string text,
            DateTime date,
            TimeSpan hour,
            string[] values,
            Content content,
            IReadOnlyDictionary<long, string> dictionary
        )
        {
            Id = id;
            Text = text;
            Date = date;
            Hour = hour;
            Values = values;
            Content = content;
            Dictionary = dictionary;
        }

        [Index(0)]
        public virtual long Id { get; set; }

        [Index(1)]
        public virtual string Text { get; set; }

        [Index(2)]
        public virtual DateTime Date { get; set; }

        [Index(3)]
        public virtual TimeSpan Hour { get; set; }

        [Index(4)]
        public virtual string[] Values { get; set; }

        [Index(5)]
        public virtual IReadOnlyDictionary<long, string> Dictionary { get; set; }

        [IgnoreFormat]
        [Index(6)]
        public virtual Content Content { get; set; }

        public static LargeModel CreateInstance(long id)
        {
            return new LargeModel
            (
                id,
                $"AAAAAAAAA BBBBBBB CCCCCCC DDDDDDD EEEEEEEE FFFFFFFF GGGGGGGG {id}",
                DateTime.Now,
                TimeSpan.MaxValue,
                new [] { "AAAAAAA", "BBBBBBBB", "CCCCCCCC", "DDDDDDDD", "EEEEEEEE", "FFFFFFFF", "GGGGGGGG" },
                new Content(Guid.NewGuid(), 343434343434343),
                new Dictionary<long, string> { { id, "DDDDDDDDD-EEEEEEEEEE-RRRRRRRRR-5555555-XXXXXXXXXX-3333333333" } }
            );
        }

        public override string ToString()
        {
            return $"Id: {Id} | Text: {Text}";
            //return $"       -> Id: {Id} | Text: {Text} | Date: {Date.ToString()} | Hour: {Hour.ToString()} | Values: [ {string.Join(", ", Values)} ] | Dictionary: [ Key: {Dictionary[Id]}, Value: {Dictionary[Id]} ] | Content: [ Key: {Content.Key}, Value: {Content.Value} ]";
        }
    }

    [ZeroFormattable]
    public class Content
    {
        public Content() { }

        public Content(Guid key, object value)
        {
            Key = key;
            Value = value;
        }

        [Index(0)]
        public virtual Guid Key { get; }

        [Index(1)]
        public virtual object Value { get; }
    }
}
