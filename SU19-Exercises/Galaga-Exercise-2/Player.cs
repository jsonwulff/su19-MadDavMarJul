using System.IO;
using DIKUArcade.Entities;
using DIKUArcade.EventBus;
using DIKUArcade.Graphics;
using DIKUArcade.Math;

namespace Galaga_Exercise_2 {
    public class Player : IGameEventProcessor<object> {
        private Game game;
        public Entity entity;

        public Player(Game game, DynamicShape shape, IBaseImage image)
            {
            this.game = game;
            entity = new Entity(shape, image);
        }
        
        
        // <summary>
        /// Sets player direction as given direction.
        /// </summary>
        /// <param name="direction"></param>
        public void Direction(Vec2F direction) {
            var shape = entity.Shape.AsDynamicShape();
            shape.ChangeDirection(direction);
        }

        /// <summary>
        /// Updates the movement of player object.
        /// </summary>
        public void Move() {
            Vec2F newPos = entity.Shape.AsDynamicShape().Direction + entity.Shape.Position;
            if (!(newPos.X < 0.0f ||
                  newPos.X + entity.Shape.Extent.X > 1.0f ||
                  newPos.Y + entity.Shape.Extent.Y < 0.0f ||
                  newPos.Y > 1.0f)) {
                entity.Shape.Move();
            }
        }

        /// <summary>
        /// Instantiates playerShot at the players gun's position.
        /// </summary>
        public void Shot() {
            game.playerShots.Add(
                new PlayerShot(game,
                    new DynamicShape(
                        new Vec2F(
                            entity.Shape.Position.X+entity.Shape.Extent.X/2, 
                            this.entity.Shape.Position.Y+entity.Shape.Extent.Y),
                        new Vec2F(0.008f, 0.027f)),
                    game.playerShotImage));
        }
        
        /// <summary>
        /// KeyPress handles logic for a given key sent by ProcessEvent. 
        /// </summary>
        /// <param name="key"></param>
        public void KeyPress(string key) {
            switch (key) {
            case "KEY_RIGHT":
                Direction(new Vec2F(0.01f, 0.0f));
                break;
            case "KEY_LEFT":
                Direction(new Vec2F(-0.01f, 0.0f));
                break;
            case "KEY_SPACE":
                Shot();
                break;
            }
        }
        
        /// <summary>
        /// KeyRelease handles logic when a key sent by ProcessEvent is released.
        /// </summary>
        /// <param name="key"></param>
        public void KeyRelease(string key) {
            switch (key) {
            case "KEY_RIGHT":
            case "KEY_LEFT":
                Direction(new Vec2F(0.00f, 0.0f));
                break;
            }
        }

/*       public void ProcessEvent(GameEventType eventType, GameEvent<object> gameEvent) {
            if (eventType == GameEventType.InputEvent) {
                switch (gameEvent.Parameter1) {
                case "KEY_PRESS":
                    KeyPress(gameEvent.Message);
                    break;
                case "KEY_RELEASE":
                    KeyRelease(gameEvent.Message);
                    break;
                }
            }
       }*/
        
    }
}