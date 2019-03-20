using System.Collections.Generic;
using System.IO;
using DIKUArcade;
using DIKUArcade.Entities;
using DIKUArcade.EventBus;
using DIKUArcade.Graphics;
using DIKUArcade.Math;
using DIKUArcade.Physics;
using DIKUArcade.Timers;
using Galaga_Exercise_3.GalagaEntities;
using Galaga_Exercise_3.GalagaStates;
using Galaga_Exercise_3.MovementStrategy;
using Galaga_Exercise_3.Squadrons;

namespace Galaga_Exercise_3 {
    public class Game : IGameEventProcessor<object> {
        private Window win;
        private GameTimer gameTimer;

        private StateMachine stateMachine;
        
        private Player player;
        
        public GameEventBus<object> eventBus;
        
        private List<Enemy> enemies;
        private List<Image> enemyStrides;
        private Row row;
        private ZigZagDown movementStrategy;

        public List<PlayerShot> playerShots { get; private set; }
        public Image playerShotImage { get; }

        private List<Image> explosionStrides;
        private AnimationContainer explosions;
        private int explosionLength = 500;


        private Score score;
        
        /// <summary>
        /// Constructor for Game class
        /// </summary>
        public Game() {
            win = new Window("Galaga", 500, 500);
            gameTimer = new GameTimer(60, 60);
            
            stateMachine = new StateMachine();
            
            player = new Player(this,
                new DynamicShape(new Vec2F(0.45f, 0.1f), new Vec2F(0.1f, 0.1f)),
                new Image(Path.Combine("Assets", "Images", "Player.png")));

            eventBus = GalagaBus.GetBus();
            eventBus.InitializeEventBus(new List<GameEventType> {
                GameEventType.InputEvent, // key press / key release
                GameEventType.WindowEvent, // messages to the window
                GameEventType.PlayerEvent,
                GameEventType.GameStateEvent
            });
            win.RegisterEventBus(eventBus);
            eventBus.Subscribe(GameEventType.InputEvent, this);
            eventBus.Subscribe(GameEventType.WindowEvent, this);
            eventBus.Subscribe(GameEventType.PlayerEvent, player);
            eventBus.Subscribe(GameEventType.PlayerEvent, stateMachine);
            
            enemyStrides = ImageStride.CreateStrides(4,
                Path.Combine("Assets", "Images", "BlueMonster.png"));
            enemies = new List<Enemy>();
            row = new Row(this);
            movementStrategy = new ZigZagDown();
            
            
            playerShots = new List<PlayerShot>();
            playerShotImage = new Image(
                Path.Combine("Assets", "Images", "BulletRed2.png"));
            
            explosionStrides = ImageStride.CreateStrides(8,
                Path.Combine("Assets", "Images", "Explosion.png"));
            explosions = new AnimationContainer(4);
            
            score = new Score(new Vec2F(0.01f, -0.25f), new Vec2F(0.3f, 0.3f));
        }
        
        /// <summary>
        /// IterateShots handles the logic of the playerShots. It checks for collision with enemies and
        /// deletes both enemies and shots if needed.
        /// </summary>
        public void IterateShots() {
            foreach (var shot in playerShots) {
                shot.Shape.Move();
                if (shot.Shape.Position.Y > 1.0f) {
                    shot.DeleteEntity();
                }

                foreach (Enemy enemy in row.Enemies) {
                    var shotHit = CollisionDetection.Aabb(shot.Shape.AsDynamicShape(), enemy.Shape);
                    if (shotHit.Collision) {
                        AddExplosion(enemy.Shape.Position.X, enemy.Shape.Position.Y, 
                            enemy.Shape.Extent.X, enemy.Shape.Extent.Y);
                        shot.DeleteEntity();
                        enemy.DeleteEntity();
                        score.AddPoint(100);
                    }
                }
            }
            
            var newShot = new List<PlayerShot>();
            foreach (var shot in playerShots) {
                if (!shot.IsDeleted()) {
                    newShot.Add(shot);
                }
            }

            playerShots = newShot;
            
            row.Enemies.Iterate(entity => {
                entity.RenderEntity();
            });
        }

       
        /// <summary>
        /// Instantiates the explosion animation at the given position.
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="extentX"></param>
        /// <param name="extentY"></param>
        public void AddExplosion(float posX, float posY,
            float extentX, float extentY) {
            explosions.AddAnimation(
                new StationaryShape(posX, posY, extentX, extentY), explosionLength,
                new ImageStride(explosionLength / 8, explosionStrides));
        }


        /// <summary>
        /// GameLoop utilizes the GameTimer class to ensure that the game runs at a steady speed on all systems.
        /// We update both rendering and game logic in the loop. The speed of the updates are specified in the gameTimer object.
        /// </summary>
        public void GameLoop() {
            //AddEnemies();
            row.CreateEnemies(enemyStrides);
            
            while (win.IsRunning()) {
                gameTimer.MeasureTime();
                while (gameTimer.ShouldUpdate()) {
                    win.PollEvents();
                    eventBus.ProcessEvents();
                    
                    player.Move();
                    
                    movementStrategy.MoveEnemies(row.Enemies);
                    
                    IterateShots();
                }

                if (gameTimer.ShouldRender()) {
                    win.Clear();

                    player.entity.RenderEntity();
                    
                    row.Enemies.Iterate(entity => entity.RenderEntity());
                    
                    foreach (var shot in playerShots) {
                        shot.RenderEntity();
                    }

                    explosions.RenderAnimations();
                    
                    score.RenderScore();
                    
                    win.SwapBuffers();
                }
                
                if (gameTimer.ShouldReset()) {
                    // 1 second has passed - display last captured ups and fps
                    win.Title = "Galaga | UPS: " + gameTimer.CapturedUpdates +
                                ", FPS: " + gameTimer.CapturedFrames;
                }
            }
        }

        /// <summary>
        /// KeyPress handles logic for a given key sent by ProcessEvent. 
        /// </summary>
        /// <param name="key"></param>
        public void KeyPress(string key) {
            switch (key) {
            case "KEY_ESCAPE":
                eventBus.RegisterEvent(
                    GameEventFactory<object>.CreateGameEventForAllProcessors(
                        GameEventType.WindowEvent, this, "CLOSE_WINDOW", "", ""));
                break;           
            }
        }
        
        /// <summary>
        /// KeyRelease handles logic when a key sent by ProcessEvent is released.
        /// </summary>
        /// <param name="key"></param>
        public void KeyRelease(string key) {
            switch (key) {

            }
        }
        
        /// <summary>
        /// ProcessEvent is the handler for keypresses on during runtime. It broadcasts GameEvents.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="gameEvent"></param>
        public void ProcessEvent(GameEventType eventType,GameEvent<object> gameEvent) {
            if (eventType == GameEventType.WindowEvent) {
                switch (gameEvent.Message) {
                case "CLOSE_WINDOW":
                    win.CloseWindow();
                    break;
                }
            } else if (eventType == GameEventType.InputEvent) {
                switch (gameEvent.Parameter1) {
                case "KEY_PRESS":
                    KeyPress(gameEvent.Message);
                    player.KeyPress(gameEvent.Message);
                    break;
                case "KEY_RELEASE":
                    player.KeyRelease(gameEvent.Message);
                    break;
                }
            }
        }
    }
}
