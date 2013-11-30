using System;
using System.Collections.Generic;
using System.Text;
using AnAppADay.JediSpeak.Shared;
using SpeechLib;
using System.Windows.Forms;
using System.Threading;

namespace AnAppADay.JediSpeak.Server
{

    class JediSpeak : MarshalByRefObject, IJediSpeak
    {

        public void Speak(string s)
        {
            SpVoice speech = new SpVoice();
            speech.Speak(s, SpeechVoiceSpeakFlags.SVSFDefault);
        }

    }

}
