using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace EmuInvaders.Machine
{
    public class Audio
    {
        //Port 3: (discrete sounds)
        //bit 0=UFO(repeats)         SX0 0.raw
        //bit 1=Shot                 SX1 1.raw
        //bit 2=Flash(player die)    SX2 2.raw
        //bit 3=Invader die          SX3 3.raw
        //bit 4=Extended play        SX4
        //bit 5= AMP enable          SX5
        //bit 6= NC(not wired)
        //bit 7= NC(not wired)
        //Port 4: (discrete sounds)
        //bit 0-7 shift data(LSB on 1st write, MSB on 2nd)
        private byte lastPort3Value = 0;
        private byte port3Value = 0;

        //Port 5:
        //bit 0=Fleet movement 1     SX6 4.raw
        //bit 1=Fleet movement 2     SX7 5.raw
        //bit 2=Fleet movement 3     SX8 6.raw
        //bit 3=Fleet movement 4     SX9 7.raw
        //bit 4=UFO Hit              SX10 8.raw
        //bit 5= NC(Cocktail mode control...to flip screen)
        //bit 6= NC(not wired)
        //bit 7= NC(not wired)
        private byte lastPort5Value = 0;
        private byte port5Value = 0;

        public IEnumerable<SoundType> GetSoundsToPlay()
        {
            var soundToPlay = new List<SoundType>();

            if (port3Value != lastPort3Value)
            {
                if ((port3Value & 1 << 0) == 0b00000001 && (lastPort3Value & 1 << 0) == 0)
                {
                    soundToPlay.Add(SoundType.UfoStart);
                }
                else if ((port3Value & 1 << 0) == 0 && (lastPort3Value & 1 << 0) == 1 << 0)
                {
                    soundToPlay.Add(SoundType.UfoEnd);
                }

                if ((port3Value & 1 << 1) == 1 << 1 && (lastPort3Value & 1 << 1) == 0)
                {
                    soundToPlay.Add(SoundType.Shot);
                }
                if ((port3Value & 1 << 2) == 1 << 2 && (lastPort3Value & 1 << 2) == 0)
                {
                    soundToPlay.Add(SoundType.PlayerDie);
                }
                if ((port3Value & 1 << 3) == 1 << 3 && (lastPort3Value & 1 << 3) == 0)
                {
                    soundToPlay.Add(SoundType.InvaderDie);
                }

                lastPort3Value = port3Value;
            }

            if (port5Value != lastPort5Value)
            {
                if ((port5Value & 1 << 0) == 1 << 0 && (lastPort5Value & 1 << 0) == 0)
                {
                    soundToPlay.Add(SoundType.FleetMovement1);
                }
                if ((port5Value & 1 << 1) == 1 << 1 && (lastPort5Value & 1 << 1) == 0)
                {
                    soundToPlay.Add(SoundType.FleetMovement2);
                }
                if ((port5Value & 1 << 2) == 1 << 2 && (lastPort5Value & 1 << 2) == 0)
                {
                    soundToPlay.Add(SoundType.FleetMovement3);
                }
                if ((port5Value & 1 << 3) == 1 << 3 && (lastPort5Value & 1 << 3) == 0)
                {
                    soundToPlay.Add(SoundType.FleetMovement4);
                }
                if ((port5Value & 1 << 4) == 1 << 4 && (lastPort5Value & 1 << 4) == 0)
                {
                    soundToPlay.Add(SoundType.UfoHit);
                }

                lastPort5Value = port5Value;
            }

            return soundToPlay;
        }

        internal void WritePort3(byte value)
        {
            port3Value = value;
        }
        internal void WritePort5(byte value)
        {
            port5Value = value;
        }
    }


    
    public enum SoundType
    {
        UfoStart = 0,
        UfoEnd = -1,
        Shot = 1,
        PlayerDie = 2,
        InvaderDie = 3,
        FleetMovement1 = 4,
        FleetMovement2 = 5,
        FleetMovement3 = 6,
        FleetMovement4 = 7,
        UfoHit = 8
    }
}
