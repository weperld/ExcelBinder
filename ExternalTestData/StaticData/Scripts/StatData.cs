using System;
using System.Collections.Generic;
using System.IO;

namespace ExternalGameData
{
    [Serializable]
    public sealed partial class StatData : StaticDataBase
    {
        public int id { get; private set; }
        public int Power { get; private set; }
        public IReadOnlyList<int> Def { get; private set; }
        public double Atk_Damge_Increase_Rate { get; private set; }
        public double Atk_Damge_Decrease_Rate { get; private set; }
        public IReadOnlyList<double> Rcv_Damge_Increase_Rate { get; private set; }
        public double Attack_Speed_Increase_Rate { get; private set; }
        public double Skill_Speed_Increase_Rate { get; private set; }
        public int Max_Hp { get; private set; }
        public int Current_Hp { get; private set; }
        public int Skill_Cooltime_INT_Decrease { get; private set; }
        public double Skill_Cooltime_per_Decrease { get; private set; }
        public int Current_Skill_Cooltime { get; private set; }
        public int Move_Speed { get; private set; }
        public int Attack_range { get; private set; }

        public override void Deserialize(BinaryReader reader)
        {
            id = reader.ReadInt32();
            Power = reader.ReadInt32();
            int defCount = reader.ReadInt32();
            var tempDef = new int[defCount];
            for(int i = 0; i < defCount; i++) tempDef[i] = reader.ReadInt32();
            Def = tempDef;
            Atk_Damge_Increase_Rate = reader.ReadDouble();
            Atk_Damge_Decrease_Rate = reader.ReadDouble();
            int rcv_damge_increase_rateCount = reader.ReadInt32();
            var tempRcv_Damge_Increase_Rate = new double[rcv_damge_increase_rateCount];
            for(int i = 0; i < rcv_damge_increase_rateCount; i++) tempRcv_Damge_Increase_Rate[i] = reader.ReadDouble();
            Rcv_Damge_Increase_Rate = tempRcv_Damge_Increase_Rate;
            Attack_Speed_Increase_Rate = reader.ReadDouble();
            Skill_Speed_Increase_Rate = reader.ReadDouble();
            Max_Hp = reader.ReadInt32();
            Current_Hp = reader.ReadInt32();
            Skill_Cooltime_INT_Decrease = reader.ReadInt32();
            Skill_Cooltime_per_Decrease = reader.ReadDouble();
            Current_Skill_Cooltime = reader.ReadInt32();
            Move_Speed = reader.ReadInt32();
            Attack_range = reader.ReadInt32();
        }
    }

    [Serializable]
    public sealed partial class StatDataTable : DataTableBase<int, StatData>
    {
        protected override int GetKey(StatData data) => data.id;
    }
}
