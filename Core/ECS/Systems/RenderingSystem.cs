using System.Collections.Generic;
using System.Numerics;
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

        public RenderingSystem(ComponentManager componentManager, IImageService imageService, IRenderService renderService, ICameraService cameraService)
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
            Color4 debugRectColor = new Color4(0.5f, 0f, 0.5f, 1f); 
    // Batch processing for OpenGL renderer
    if (PlatformInfo.RendererType == RendererType.OpenGlRenderer)
    {
        var glHandle = renderService.OpenGLHandle2D;
        var modelMatrices = new List<Matrix4>();
        var textureIds = new List<int>();

        foreach (var entityId in sprites.Keys)
        {
            if (positions.TryGetValue(entityId, out var position) && sprites.TryGetValue(entityId, out var sprite))
            {
                // Retrieve sprite dimensions
                float spriteWidth = sprite.Sprite.Width;   
                float spriteHeight = sprite.Sprite.Height;

                // Calculate the offset to center the sprite
                Vector2 offset = new Vector2(spriteWidth * 0.5f, spriteHeight * 0.5f) * sprite.Scale;

                // Adjust the position by subtracting the offset
                OpenTK.Mathematics.Vector2 adjustedPosition = new OpenTK.Mathematics.Vector2(
                    position.Position.X - offset.X,
                    position.Position.Y - offset.Y
                );

                // Create the translation matrix with the adjusted position
                var matrixTranslate = MathHelper.GetMatrixTranslation(adjustedPosition, sprite.Sprite.Width);
                modelMatrices.Add(matrixTranslate);
                textureIds.Add((int)sprite.Sprite.TextureId);

                // **Begin Debug Rectangle Rendering**
                // Calculate the four corners of the sprite
                Vector2 topLeft = position.Position - offset;
                Vector2 topRight = topLeft + new Vector2(spriteWidth * sprite.Scale.X, 0);
                Vector2 bottomRight = topLeft + new Vector2(spriteWidth * sprite.Scale.X, spriteHeight * sprite.Scale.Y);
                Vector2 bottomLeft = topLeft + new Vector2(0, spriteHeight * sprite.Scale.Y);

                // Convert to OpenTK's Vector2
                OpenTK.Mathematics.Vector2 tl = new OpenTK.Mathematics.Vector2(topLeft.X, topLeft.Y);
                OpenTK.Mathematics.Vector2 tr = new OpenTK.Mathematics.Vector2(topRight.X, topRight.Y);
                OpenTK.Mathematics.Vector2 br = new OpenTK.Mathematics.Vector2(bottomRight.X, bottomRight.Y);
                OpenTK.Mathematics.Vector2 bl = new OpenTK.Mathematics.Vector2(bottomLeft.X, bottomLeft.Y);

                // Draw lines between the corners to form a rectangle
                renderService.DrawLine(tl, tr, debugRectColor);
                renderService.DrawLine(tr, br, debugRectColor);
                renderService.DrawLine(br, bl, debugRectColor);
                renderService.DrawLine(bl, tl, debugRectColor);
                // **End Debug Rectangle Rendering**
            }
        }

        if (modelMatrices.Count > 0)
        {
            imageService.DrawTexturesGLBatched(
                glHandle, 
                textureIds.ToArray(), 
                cameraService.GetActiveCamera(), 
                modelMatrices.ToArray()
            );
        }
    }

            // Render using SDL Renderer
            if (PlatformInfo.RendererType == RendererType.SDLRenderer)
            {
                foreach (var entityId in sprites.Keys)
                {
                    if (positions.TryGetValue(entityId, out var position) &&
                        sprites.TryGetValue(entityId, out var sprite))
                    {
                        var pos = new Vector2(position.Position.X, position.Position.Y);
                        sprite.Sprite.Render(renderer, pos, 0, sprite.Scale);
                    }
                }
            }
        }
    }
}
