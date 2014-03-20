using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices; 
namespace ConsoleApplication1
{
    class WavePlayerLib
    {

        /*
         * Get the time (in terms of seconds) when the wav is playing at the moment.
         */
        [DllImport("WavePlayerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int currentSec();                         
        /*
         * Set volume of the audio device. simpleVol:int range is [0 .. 100]
         */
        [DllImport("WavePlayerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setVolume(int simpleVol);      
        /*
         * Pause the playing of wav.
         */
        [DllImport("WavePlayerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PauseWave();
        /*
         * Resume the playing of wav.
         */
        [DllImport("WavePlayerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ResumeWave();
        /*
         * Skip to some position in terms of seconds.
         */
        [DllImport("WavePlayerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void skipTo(int secPosition);
        /*
         * Stop the playing of wav.
         */ 
        [DllImport("WavePlayerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StopWave();
        /*
         * Start the playing of wav file.
         */
        [DllImport("WavePlayerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PlayWave(StringBuilder filename);

        


    }
}
