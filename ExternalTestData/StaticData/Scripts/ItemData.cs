using System;
using System.Collections.Generic;
using System.IO;

namespace ExternalGameData
{
    [Serializable]
    public sealed partial class ItemData : StaticDataBase
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int Price { get; private set; }
        public IReadOnlyList<int> Skill { get; private set; }
        public IEnumerable<SkillData> Skill_Ref
        {
            get
            {
                foreach (var x in Skill)
                {
                    yield return StaticDataManager.Instance.Skill[x];
                }
            }
        }

        public override void Deserialize(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            Name = reader.ReadString();
            Price = reader.ReadInt32();
            int skillCount = reader.ReadInt32();
            var tempSkill = new int[skillCount];
            for(int i = 0; i < skillCount; i++) tempSkill[i] = reader.ReadInt32();
            Skill = tempSkill;
        }
    }

    [Serializable]
    public sealed partial class ItemDataTable : DataTableBase<int, ItemData>
    {
        protected override int GetKey(ItemData data) => data.Id;
    }
}
