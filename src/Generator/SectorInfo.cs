using PixelsOfDoom.Config;
using System;

namespace PixelsOfDoom.Generator
{
    public struct SectorInfo
    {
        public SettingsPixelType SectorType { get; }

        public int CeilingHeight { get; }
        public int FloorHeight { get; }
        public int LineSpecial { get; }
        public int SectorSpecial { get; }

        public int LightLevel { get; }

        public string CeilingTexture { get; }
        public string FloorTexture { get; }
        public string SpecialFlatTexture { get; }
        public string WallTexture { get; }
        public string WallTextureAlt { get; }
        public string WallTextureAlt2 { get; }


        public SectorInfo(SettingsPixel pixel, bool sectorIsADoor = false)
        {
            SectorType = pixel.PixelType;

            CeilingHeight = Toolbox.RandomFromArray(pixel.CeilingHeight);
            FloorHeight = Math.Min(CeilingHeight, Toolbox.RandomFromArray(pixel.FloorHeight));
            LineSpecial = Math.Max(0, Toolbox.RandomFromArray(pixel.LineSpecial));
            SectorSpecial = Math.Max(0, Toolbox.RandomFromArray(pixel.SectorSpecial));

            LightLevel = Toolbox.Clamp(Toolbox.RandomFromArray(pixel.LightLevel), 0, 255);

            CeilingTexture = Toolbox.RandomFromArray(pixel.CeilingTexture);
            FloorTexture = Toolbox.RandomFromArray(pixel.FloorTexture);
            SpecialFlatTexture = Toolbox.RandomFromArray(pixel.SpecialFlatTexture);
            WallTexture = Toolbox.RandomFromArray(pixel.WallTexture);
            WallTextureAlt = Toolbox.RandomFromArray(pixel.WallTextureAlt);
            WallTextureAlt2 = Toolbox.RandomFromArray(pixel.WallTextureAlt2);

            if (sectorIsADoor)
            {
                CeilingHeight = FloorHeight;
            }
        }
    }
}
