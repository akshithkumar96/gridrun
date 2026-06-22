using System;
using System.Collections.Generic;

namespace GridRun.Services
{
    [Serializable]
    public sealed class PlayerProfile
    {
        public int Credits = 3500;
        public int Battery = 5;
        public int MaxHpBonus;
        public int AttackBonus;
        public int SkillSlots = 3;
        public List<string> UnlockedSkills = new List<string> { "brute_force", "firewall", "decrypt" };
        public List<string> ClaimedBattlePassRewards = new List<string>();
        public List<string> OwnedPremiumPacks = new List<string>();
    }
}
