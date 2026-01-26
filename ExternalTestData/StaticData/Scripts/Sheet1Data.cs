using System;
using System.Collections.Generic;
using System.IO;

namespace ExternalGameData
{
    [Serializable]
    public sealed partial class Sheet1Data : StaticDataBase
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Desc { get; private set; }
        public double Price { get; private set; }

        public override void Deserialize(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            Name = reader.ReadString();
            Desc = reader.ReadString();
            Price = reader.ReadDouble();
        }
    }

    [Serializable]
    public sealed partial class Sheet1DataTable : DataTableBase<int, Sheet1Data>
    {
        protected override int GetKey(Sheet1Data data) => data.Id;
    }
}
