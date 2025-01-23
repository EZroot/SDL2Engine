using System.Numerics;
using Box2DSharp.Common;
using OpenTK.Mathematics;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.ECS.Components;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;
using Vector2 = System.Numerics.Vector2;

namespace SDL2Engine.Core.ECS.Systems;

public class RenderingSystem
{
    IImageService m_imageService;
    IRenderService m_renderService;
    public RenderingSystem(IImageService imageService, IRenderService renderService)
    {
        m_imageService = imageService;
        m_renderService = renderService;
    }
    
    public void Render(IEnumerable<Entity> entities, 
        ComponentManager<PositionComponent> positionComponents, 
        ComponentManager<SpriteComponent> spriteComponents,
        nint renderer,
        ICameraService cameraService)
    {
        var positions = positionComponents.GetAllComponents(entities).ToDictionary(p => p.EntityId, p => p.Component);
        var sprites = spriteComponents.GetAllComponents(entities).ToDictionary(s => s.EntityId, s => s.Component);

        // Batch processing for OpenGL renderer
        if (PlatformInfo.RendererType == RendererType.OpenGlRenderer)
        {
            var glHandle = m_renderService.OpenGLHandle2D;
            var modelMatrices = new List<Matrix4>();
            var textureIds = new List<int>();

            foreach (var entity in entities)
            {
                if (positions.TryGetValue(entity.Id, out var position) && 
                    sprites.TryGetValue(entity.Id, out var sprite))
                {
                    //todo: fix proper scale here 
                    var matrixTranslate = MathHelper.GetMatrixTranslation(position.Position, sprite.Scale.X);
                    modelMatrices.Add(matrixTranslate);
                    textureIds.Add((int)sprite.Sprite.TextureId);
                }
            }

            if (modelMatrices.Count > 0)
            {
                m_imageService.DrawTexturesGLBatched(glHandle, textureIds.ToArray(), cameraService.GetActiveCamera(), modelMatrices.ToArray());
            }
        }

        if (PlatformInfo.RendererType == RendererType.SDLRenderer)
        {
            foreach (var entity in entities)
            {
                if (positions.TryGetValue(entity.Id, out var position) &&
                    sprites.TryGetValue(entity.Id, out var sprite))
                {
                    var pos = new Vector2(position.Position.X, position.Position.Y);
                    sprite.Sprite.Render(renderer, pos, 0, sprite.Scale);
                }
            }
        }

    }
}