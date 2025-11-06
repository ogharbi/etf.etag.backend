using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VC.AG.Models.Enums
{
    public enum MailType
    {
        None = 0,
        Information = 2,
        InterviewToStartReminder = 3,
        QInterviewToStartReminder = 4,
        QInterviewNotStartedReminder = 5,
        Reminder = 6
    }
}
