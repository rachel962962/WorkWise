using System;
using System.Collections.Generic;

namespace DTO
{
    public enum AbsenceType
    {
        חופשה,
        מחלה,
        אחר
    }
    public partial class WorkerAbsenceDTO
    {
        public int AbsenceId { get; set; }

        public int WorkerId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AbsenceType Reason { get; set; }

    }
}
