using System;
using System.Collections.Generic;
using System.IO;

namespace ExternalGameData
{
    [Serializable]
    public sealed partial class SkillData : StaticDataBase
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int Power { get; private set; }

        public override void Deserialize(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            Name = reader.ReadString();
            Power = reader.ReadInt32();
        }
    }

    [Serializable]
    public sealed partial class SkillDataTable : DataTableBase<int, SkillData>
    {
        protected override int GetKey(SkillData data) => data.Id;
    }
}
