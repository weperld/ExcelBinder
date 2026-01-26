using System;
using System.Collections.Generic;
using System.IO;

namespace ExternalGameData
{
    [Serializable]
    public sealed partial class CharacterData : StaticDataBase
    {
        public int id { get; private set; }
        public string name { get; private set; }
        public int stat { get; private set; }
        public StatDataData stat_Ref => StaticDataManager.Instance.StatData[stat];
        public int stat_growth { get; private set; }
        public StatDataData stat_growth_Ref => StaticDataManager.Instance.StatData[stat_growth];

        public override void Deserialize(BinaryReader reader)
        {
            id = reader.ReadInt32();
            name = reader.ReadString();
            stat = reader.ReadInt32();
            stat_growth = reader.ReadInt32();
        }
    }

    [Serializable]
    public sealed partial class CharacterDataTable : DataTableBase<int, CharacterData>
    {
        protected override int GetKey(CharacterData data) => data.id;
    }
}
