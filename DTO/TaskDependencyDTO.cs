using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TaskDependencyDTO
    {
        public int TaskId { get; set; }

        public int DependentTaskId { get; set; }
    }
}
