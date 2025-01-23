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

        public void Render(nint renderer)
        {
            var positions = componentManager.GetComponentDictionary<PositionComponent>();
            var sprites = componentManager.GetComponentDictionary<SpriteComponent>();

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
                        // TODO: Implement proper scaling logic
                        OpenTK.Mathematics.Vector2 vec =
                            new OpenTK.Mathematics.Vector2(position.Position.X, position.Position.Y);
                        var matrixTranslate = MathHelper.GetMatrixTranslation(vec, sprite.Scale.X);
                        modelMatrices.Add(matrixTranslate);
                        textureIds.Add((int)sprite.Sprite.TextureId);
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
