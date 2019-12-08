using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using NAudio.Wave;

using System.Linq;
using System.Threading.Tasks;

namespace FunMain
{
    public class SoundControls
    {
        public void PlaySound()

        {

            var path = Directory.GetCurrentDirectory();

            //var file = Path.Combine(path, "AccessDenied.mp3");
            var file = @"c:\Windows\Media\chimes.wav";
            using (var audioFile = new AudioFileReader(file))

            using (var outputDevice = new WaveOutEvent())

            {

                outputDevice.Init(audioFile);

                outputDevice.Play();

                while (outputDevice.PlaybackState == PlaybackState.Playing)

                {

                    Thread.Sleep(1000);

                }

            }

        }
    }
}
