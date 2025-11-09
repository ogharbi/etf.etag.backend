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
        InterviewNotStartedReminder = 4,
        QInterviewToStartReminder = 5,
        QInterviewNotStartedReminder = 6,
        Reminder = 7
    }
}
