using Bolt.Models;

namespace Bolt.Interfaces
{
    public interface IModDeploymentService
    {
        bool DeployModification(GameModel game, ModificationModel modification);
        bool DeployModifications(GameModel game, IEnumerable<ModificationModel> modifications);
        bool RevertModification(GameModel game, ModificationModel modification);
        bool RevertModifications(GameModel game, IEnumerable<ModificationModel> modifications);
    }
}