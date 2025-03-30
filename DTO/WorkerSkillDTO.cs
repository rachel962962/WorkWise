using System;
using System.Collections.Generic;

namespace DTO
{
    public enum ProficiencyLevel
    {
        None,
        מתחיל,
        בינוני,
        מומחה

    }
    public partial class WorkerSkillDTO
    {
        public int WorkerId { get; set; }

        public int SkillId { get; set; }

        public ProficiencyLevel ProficiencyLevel { get; set; }

    }
}