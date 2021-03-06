using DIKUArcade.Entities;
using Galaga_Exercise_2.GalagaEnities.Enemy;

namespace Galaga_Exercise_2.MovementStrategy {
    public interface IMovementStrategy {
        void MoveEnemy(Enemy enemy);
        void MoveEnemies(EntityContainer<Enemy> enemies);
    }
}