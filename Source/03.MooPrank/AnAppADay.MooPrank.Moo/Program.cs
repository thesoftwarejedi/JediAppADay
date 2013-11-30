using System;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Reflection;
using WaveLib.AudioMixer;

namespace AnAppADay.MooPrank.Moo
{

    static class Program
    {
     
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                Random r = new Random();
                int w1 = 0;
                int w2 = 0;
                try
                {
                    w1 = Int32.Parse(args[0]);
                    w2 = Int32.Parse(args[1]);
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid args, no args = moo now, 2 args = random wait between moos (in seconds)");
                    return;
                }
                if (w1 > w2 || w1 < 1 || w2 < 1)
                {
                    MessageBox.Show("Invalid args, no args = moo now, 2 args = random wait min and max between moos (in seconds)");
                    return;
                }
                while (true)
                {
                    int wait = r.Next(w1, w2);
                    System.Threading.Thread.Sleep(wait * 1000);
                    PlayMoo();
                }
            }
            else if (args.Length == 0)
            {
                PlayMoo();
            }
            else
            {
                MessageBox.Show("no args = moo now, 2 args = random wait between moos (in seconds)");
            }
        }

        private static void PlayMoo()
        {
            try
            {
                Mixers mixers = new Mixers();
                mixers.Playback.DeviceId = mixers.Playback.DeviceIdDefault;
                MixerLine line = mixers.Playback.Lines.GetMixerFirstLineByComponentType(MIXERLINE_COMPONENTTYPE.DST_SPEAKERS);
                if (line != null)
                {
                    line.Volume = line.VolumeMax;
                    line.Mute = false;
                }
                Stream wav = Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.MooPrank.Moo.moo.wav");
                SoundPlayer sp = new SoundPlayer(wav);
                sp.PlaySync();
            }
            catch (Exception)
            {
                //quiet!  we're rogue!
            }
        }

    }

}