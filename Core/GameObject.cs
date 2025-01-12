using System.Numerics;
using SDL2;
using Box2DSharp.Dynamics; 
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core
{
    public class GameObject
    {
        // Basic transform properties
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;

        // Physics (optional)
        public Body PhysicsBody { get; set; }  // Box2DSharp body

        // Visual / Rendering properties
        public int TextureId { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }

        /// <summary>
        /// Sync position/rotation from PhysicsBody if present
        /// </summary>
        public virtual void Update(float deltaTime)
        {
            if (PhysicsBody != null)
            {
                // In Box2DSharp, Body.Position is in “meters” (if you are using physics scaling)
                // So you might need to multiply by your pixel-to-meter ratio.
                Position = PhysicsBody.GetPosition();
                Rotation = PhysicsBody.GetAngle(); 
            }
        }

        /// <summary>
        /// Example Render method. Uses your asset manager to draw.
        /// </summary>
        public virtual void Render(nint renderer, IServiceAssetManager assetManager, IServiceCameraService cameraService = null)
        {
            if (TextureId == 0) return;

            // We can calculate scaled width/height
            int scaledWidth = (int)(OriginalWidth * Scale.X);
            int scaledHeight = (int)(OriginalHeight * Scale.Y);

            // Create an SDL rect for the destination
            var destRect = new SDL.SDL_Rect
            {
                x = (int)Position.X,
                y = (int)Position.Y,
                w = scaledWidth,
                h = scaledHeight
            };

            // If you have a camera system, you might want to offset by the camera
            // or pass the rect to a Draw method that applies camera transformations
            if (cameraService != null)
            {
                var camera = cameraService.GetActiveCamera();
                assetManager.DrawTexture(renderer, TextureId, ref destRect, camera);
            }
            else
            {
                assetManager.DrawTexture(renderer, TextureId, ref destRect);
            }
        }
    }
}
