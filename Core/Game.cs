using System.Numerics;
using SDL2;
using SDL2Engine.Core.Addressables;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Windowing.Interfaces;

namespace SDL2Engine.Core;

public class Game : IGame
{
    private const string RESOURCES_FOLDER = "/home/anon/Repos/SDL2Engine/resources";
    private const string SOUND_FOLDER = "/home/anon/Music";

    private const int ENGINE_VOLUME = 24; // 0 - 128

    private readonly IServiceWindowService m_windowService;
    private readonly IServiceRenderService m_renderService;
    private readonly IServiceGuiRenderService m_guiRenderService;
    private readonly IServiceGuiWindowService m_guiWindowBuilder;
    private readonly IVariableBinder m_guiVariableBinder;
    private readonly IServiceAssetManager m_assetManager;
    private readonly IServiceAudioLoader m_audioLoader;
    private readonly IServiceCameraService m_cameraService;

    public Game(IServiceWindowService windowService,
        IServiceRenderService renderService,
        IServiceGuiRenderService guiRenderService,
        IServiceAssetManager assetManager,
        IServiceGuiWindowService guiWindowBuilder,
        IVariableBinder guiVariableBinder,
        IServiceAudioLoader audioLoader,
        IServiceCameraService cameraService)
    {
        m_windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        m_renderService = renderService ?? throw new ArgumentNullException(nameof(renderService));
        m_guiRenderService = guiRenderService ?? throw new ArgumentNullException(nameof(guiRenderService));
        m_guiWindowBuilder = guiWindowBuilder ?? throw new ArgumentNullException(nameof(guiWindowBuilder));
        m_assetManager = assetManager ?? throw new ArgumentNullException(nameof(assetManager));
        m_guiVariableBinder = guiVariableBinder ?? throw new ArgumentNullException(nameof(guiVariableBinder));
        m_audioLoader = audioLoader ?? throw new ArgumentNullException(nameof(audioLoader));
        m_cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
    }

    private Vector2 m_position, m_startPosition, m_originalScale;
    private SDL.SDL_Rect m_dstRectAsh;
    private AssetManager.TextureData m_spriteTexture;
    private float m_currentScale;
    private AssetManager.TextureData[] m_spriteTexturePokemans;
    private List<Vector2> m_originalScales = new List<Vector2>();
    private List<float> m_currScales = new List<float>();
    private List<SDL.SDL_Rect> m_dstRects = new List<SDL.SDL_Rect>();
    
    
    private float m_bandIntensityMod = 1.75f;
    private int m_rectSpacing = 1;
    private int m_maxRectHeight = 40;
    private int m_rectWidth = 3;

    private float m_maxAmplitude;
    private float[] m_previousHeights;
    private float m_smoothingFactor; // Smoothing factor between 0.1 and 0.3
    private int m_initialRectStartY;
    
    public void Initialize()
    {
        var renderer = m_renderService.GetRenderer();
        //Sprite Test
        m_spriteTexture = m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ashh.png");
        m_dstRectAsh = new SDL.SDL_Rect { x = 0, y = 0, w = m_spriteTexture.Width, h = m_spriteTexture.Height };
        m_startPosition = new Vector2(48, 174);
        m_originalScale = new Vector2(m_spriteTexture.Width, m_spriteTexture.Height);
        m_position = m_startPosition;
        var currentScale = 1.0f;

        //Lil pokemans
        m_spriteTexturePokemans = new AssetManager.TextureData[]
        {
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
        };


        for (var i = 0; i < m_spriteTexturePokemans.Length; i++)
        {
            var width = m_spriteTexturePokemans[i].Width;
            var height = m_spriteTexturePokemans[i].Height;
            int row = i / 10;
            int col = i % 10;
            var spacing = -20;
            var startPos = new Vector2(330 + (col * (width + spacing)), 0 + (row * (height + spacing)));

            // var startPos = new Vector2(150 + (i*width + width + 64), 60 * j+i);
            var rec = new SDL.SDL_Rect { x = (int)startPos.X, y = (int)startPos.Y, w = width, h = height };
            m_dstRects.Add(rec);
            m_currScales.Add(1.0f);
            m_originalScales.Add(new Vector2(width, height));
        }

        var songPath = SOUND_FOLDER + "/pokemon.wav"; //"/skidrow-portal.wav"; 

        // Music Test
        // var song = m_assetManager.LoadSound(songPath, Addressables.AudioLoader.AudioType.Music);
        // m_assetManager.PlaySound(song, 16, true);

        // Soundfx Test
        var song = m_assetManager.LoadSound(songPath);
        m_assetManager.PlaySound(song, ENGINE_VOLUME);

        // Audio player rects
        m_bandIntensityMod = 1.75f;
        m_rectSpacing = 1;
        m_maxRectHeight = 40;
        m_rectWidth = 3;

        m_maxAmplitude = 0f;
        m_previousHeights = new float[m_audioLoader.FrequencyBands.Count];
        m_smoothingFactor = .3f; // Smoothing factor between 0.1 and 0.3
        m_initialRectStartY = 200; //(int)(m_maxRectHeight / 1.75f + m_maxRectHeight / 2);
    }

    public void Update(float deltaTime)
    {

        if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_w))
            m_position.Y -= 20f * Time.DeltaTime;
        if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_a))
            m_position.X -= 20f * Time.DeltaTime;
        if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_s))
            m_position.Y += 20f * Time.DeltaTime;
        if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_d))
            m_position.X += 20f * Time.DeltaTime;

        if (m_dstRectAsh.x != (int)m_position.X || m_dstRectAsh.y != (int)m_position.Y)
        {
            // Debug.Log($"X:{position.X} Y:{position.Y}");
            m_dstRectAsh.x = (int)(m_position.X);
            m_dstRectAsh.y = (int)(m_position.Y);
        }
    }

    public void Render()
    {
        var renderer = m_renderService.GetRenderer();
        var camera = m_cameraService.GetActiveCamera();
        
        #region DrawPokemons

        var baseScale = 0.75f;
        var amplitude = 0f;

        // Trying to grab some vocals here
        for (var i = 4; i < 16; i++)
        {
            var index = i.ToString();
            amplitude += m_audioLoader.GetAmplitudeByName(index);
        }

        var scaleFactor = baseScale + amplitude; // A highre freq band, to hopefully grab vocals
        m_currentScale = MathHelper.Lerp(m_currentScale, scaleFactor, 0.1f);
        var maxScale = 3f;
        m_currentScale = Math.Min(m_currentScale, maxScale);
        m_dstRectAsh.w = (int)(m_originalScale.X * m_currentScale);
        m_dstRectAsh.h = (int)(m_originalScale.Y * m_currentScale);

        var pokemansBaseScale = 0.5f;
        var pokemansMaxScale = 5f;

        for (var i = 0; i < m_spriteTexturePokemans.Length; i++)
        {
            var pulseOffset = (i * 0.5f) % MathHelper.TwoPi;
            // Grab low freq band for the bass
            var dynamicScaleFactor = pokemansBaseScale
                                     + m_audioLoader.GetAmplitudeByName("1")
                                     * (5f + (float)Math.Sin(Time.TotalTime + pulseOffset));

            m_currScales[i] = MathHelper.Lerp(m_currScales[i], dynamicScaleFactor, 0.1f);
            m_currScales[i] = Math.Min(m_currScales[i], pokemansMaxScale);
            var ogScale = m_originalScales[i];
            var bounceX = 10f * (float)Math.Sin(Time.TotalTime * 2f + i * 0.5f);
            var bounceY = 5f * (float)Math.Cos(Time.TotalTime * 3f + i * 0.3f);
            var rec = m_dstRects[i];
            rec.w = (int)(ogScale.X * m_currScales[i]);
            rec.h = (int)(ogScale.Y * m_currScales[i]);
            rec.x += (int)bounceX;
            rec.y += (int)bounceY;
            m_assetManager.DrawTexture(renderer, m_spriteTexturePokemans[i].Id, ref rec, camera);
        }

        m_assetManager.DrawTexture(renderer, m_spriteTexture.Id, ref m_dstRectAsh);

        #endregion

        #region DrawAudioSynth

        // Audio synthesizer
        var frequencyBands = m_audioLoader.FrequencyBands;
        var bandRectSize = (m_rectWidth + m_rectSpacing) * frequencyBands.Count;
        var initialRectStartX =  400;
        // initialRectStartX -= m_windowWidth - bandRectSize / 2;

        foreach (var bandPair in m_audioLoader.FrequencyBands)
        {
            var bandAmplitude = m_audioLoader.GetAmplitudeByName(bandPair.Key);
            if (bandAmplitude > m_maxAmplitude)
            {
                m_maxAmplitude = bandAmplitude;
            }
        }

        foreach (var bandPair in frequencyBands)
        {
            var amp = m_audioLoader.GetAmplitudeByName(bandPair.Key);
            var bandIndex = int.Parse(bandPair.Key);
            var bandAmplitude = amp * m_bandIntensityMod;
            var targetHeight = (int)((bandAmplitude / m_maxAmplitude) * m_maxRectHeight);
            targetHeight = Math.Clamp(targetHeight, 0, m_maxRectHeight);

            var smoothedHeight = (int)(m_previousHeights[bandIndex] * (1 - m_smoothingFactor) +
                                       targetHeight * m_smoothingFactor);
            m_previousHeights[bandIndex] = smoothedHeight;
            var currentRectX = initialRectStartX + (bandIndex * (m_rectWidth + m_rectSpacing));

            var ratio = smoothedHeight / (float)m_maxRectHeight;
            var minHue = 0.7f; //0.45f;
            var maxHue = 0.85f; //0.7f;
            var hue = (minHue + (maxHue - minHue) * ratio) * 360;
            var (red, green, blue) = ColorHelper.HsvToRgb(hue, 1f, 1.0f);

            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
            SDL.SDL_Rect backgroundRect = new SDL.SDL_Rect
            {
                x = currentRectX - m_rectSpacing / 2,
                y = m_initialRectStartY - (smoothedHeight / 2) -
                    (m_rectSpacing / 2),
                w = m_rectWidth + m_rectSpacing,
                h = smoothedHeight + m_rectSpacing
            };
            SDL.SDL_RenderFillRect(renderer, ref backgroundRect);

            SDL.SDL_Rect rect = new SDL.SDL_Rect
            {
                x = currentRectX,
                y = m_initialRectStartY - (smoothedHeight / 2),
                w = m_rectWidth,
                h = smoothedHeight
            };
            SDL.SDL_SetRenderDrawColor(renderer, red, green, blue, 255);
            SDL.SDL_RenderFillRect(renderer, ref rect);
        }

        #endregion

    }

    public void Shutdown()
    {
        m_assetManager.UnloadTexture(m_spriteTexture.Id);
        for (var i = 0; i < m_spriteTexturePokemans.Length; i++)
        {
            m_assetManager.UnloadTexture(m_spriteTexturePokemans[i].Id);
        }
    }
}