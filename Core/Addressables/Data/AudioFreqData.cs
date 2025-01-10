namespace SDL2Engine.Core.Addressables.Data
{
    public struct AudioFeqData
    {
        public float[] FreqBandData { get;  private set; }

        public AudioFeqData(params float[] freqBands)
        {
            FreqBandData = freqBands;
        }
    }
}