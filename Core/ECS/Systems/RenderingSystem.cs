using OpenTK.Mathematics;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.ECS.Components;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;
using Vector2 = System.Numerics.Vector2;

namespace SDL2Engine.Core.ECS.Systems
{
    public class RenderingSystem : ISystem
    {
        private readonly ComponentManager componentManager;
        private readonly IImageService imageService;
        private readonly IRenderService renderService;
        private readonly ICameraService cameraService;

        public RenderingSystem(ComponentManager componentManager, IImageService imageService,
            IRenderService renderService, ICameraService cameraService)
        {
            this.componentManager = componentManager;
            this.imageService = imageService;
            this.renderService = renderService;
            this.cameraService = cameraService;
        }

        public void Update(float deltaTime)
        {
            // doesnt handle updates
        }

        public void Render(IRenderService renderService, ICameraService cameraService)
        {
            var positions = componentManager.GetComponentDictionary<PositionComponent>();
            var sprites = componentManager.GetComponentDictionary<SpriteComponent>();
            var renderer = renderService.RenderPtr;
            Color4 debugRectColor = new Color4(0.5f, 0f, 0.5f, 1f); // Magenta for debug rectangles

            // Obtain the camera offset if available
            Vector3 cameraOffset = Vector3.Zero;
            if (cameraService != null)
            {
                cameraOffset = cameraService.GetActiveCamera().GetOffset();
            }

            // Handle OpenGL Renderer
            if (PlatformInfo.RendererType == RendererType.OpenGlRenderer)
            {
                var glHandle = renderService.OpenGLHandle2D;
                var modelMatrices = new List<Matrix4>();
                var textureIds = new List<int>();

                foreach (var entityId in sprites.Keys)
                {
                    if (positions.TryGetValue(entityId, out var position) &&
                        sprites.TryGetValue(entityId, out var sprite))
                    {
                        // Retrieve sprite dimensions
                        float spriteWidth = sprite.Sprite.Width;
                        float spriteHeight = sprite.Sprite.Height;

                        var centeredPosition = new OpenTK.Mathematics.Vector2(
                            position.Position.X - (spriteWidth * 0.5f * sprite.Scale.X),
                            position.Position.Y - (spriteHeight * 0.5f * sprite.Scale.Y)
                        );

                        var matrixTranslate = MathHelper.GetMatrixTranslation(centeredPosition, sprite.Sprite.Width);
                        modelMatrices.Add(matrixTranslate);
                        textureIds.Add((int)sprite.Sprite.TextureId);

                        // Vector2 dbgcenteredPosition = position.Position - cameraOffset
                        //                                                 - new Vector2(
                        //                                                     spriteWidth * 0.5f * sprite.Scale.X,
                        //                                                     spriteHeight * 0.5f * sprite.Scale.Y);
                        // Vector2 topLeft = dbgcenteredPosition;
                        // Vector2 topRight = topLeft + new Vector2(spriteWidth * sprite.Scale.X, 0);
                        // Vector2 bottomRight = topLeft +
                        //                       new Vector2(spriteWidth * sprite.Scale.X, spriteHeight * sprite.Scale.Y);
                        // Vector2 bottomLeft = topLeft + new Vector2(0, spriteHeight * sprite.Scale.Y);
                        //
                        // OpenTK.Mathematics.Vector2 tl = new OpenTK.Mathematics.Vector2(topLeft.X, topLeft.Y);
                        // OpenTK.Mathematics.Vector2 tr = new OpenTK.Mathematics.Vector2(topRight.X, topRight.Y);
                        // OpenTK.Mathematics.Vector2 br = new OpenTK.Mathematics.Vector2(bottomRight.X, bottomRight.Y);
                        // OpenTK.Mathematics.Vector2 bl = new OpenTK.Mathematics.Vector2(bottomLeft.X, bottomLeft.Y);
                        //
                        // renderService.DrawLine(tl, tr, debugRectColor);
                        // renderService.DrawLine(tr, br, debugRectColor);
                        // renderService.DrawLine(br, bl, debugRectColor);
                        // renderService.DrawLine(bl, tl, debugRectColor);
                    }
                }

                var cam = cameraService.GetActiveCamera();
                // Batch render all sprites
                if (modelMatrices.Count > 0)
                {
                    imageService.DrawTexturesGLBatched(
                        glHandle,
                        textureIds.ToArray(),
                        cam,
                        modelMatrices.ToArray()
                    );
                }
            }

            // Handle SDL Renderer
            if (PlatformInfo.RendererType == RendererType.SDLRenderer)
            {
                foreach (var entityId in sprites.Keys)
                {
                    if (positions.TryGetValue(entityId, out var position) &&
                        sprites.TryGetValue(entityId, out var sprite))
                    {
                        // Render the sprite
                        sprite.Sprite.Render(renderer, new Vector3(position.Position.X, position.Position.Y,0), 0, sprite.Scale);
                    }
                }
            }
        }
    }
}