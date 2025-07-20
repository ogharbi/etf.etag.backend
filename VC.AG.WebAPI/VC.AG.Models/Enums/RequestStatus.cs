using Microsoft.AspNetCore.Http;

namespace VC.AG.Models.Enums
{
    public enum RequestStatus
    {
        None = 1,
        NotStarted = 2,
        InProgress = 3,
        Completed = 4

    }
    public class RequestStatusStr
    {
        public const string None = "None";
        public const string NotStarted = "Non démarré";
        public const string InProgress = "En cours";
        public const string Completed = "Terminé";
    }
}
