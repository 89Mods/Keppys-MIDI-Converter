using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Enc;
using Un4seen.BassWasapi;
using Un4seen.Bass.AddOn.Midi;
using Microsoft.Win32;
using System.IO;
using Un4seen.Bass.Misc;

namespace KeppysConverterConsole
{
    class Program
    {

        public int voiceLimit = 500;
        public int freq = 44100;
        public String MIDIFile;
        public String soundfont;
        public String outputFile;
        public DSP_PeakLevelMeter _plm = null;
        public int _recHandle;
        public Boolean useSFX = false;
        public int _Encoder;

        static void Main(string[] args)
        {
            new Program().convert(args);
        }

        public Program()
        {
            
        }

        public void convert(String[] args)
        {
            Console.Out.WriteLine("Keppys MIDI Converter Console version. Converter by KaleidonKep99, console version by TheGhastModding");
            if(args[0] == "?")
            {
                Console.Out.WriteLine("Syntax is \"KeppysConverterConsole.exe [MIDI file name] [soundfont file name] [output file name] [voice Limit] [audio frequency] {-fx[true/false]}\n\nPress any key to quit.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            if (args.Length < 5)
            {
                Console.Out.WriteLine("Too less anrguments");
                Console.Out.WriteLine("Syntax is \"KeppysConverterConsole.exe [MIDI file name] [soundfont file name] [output file name] [voice Limit] [audio frequency] {-fx[true/false]}\n\nPress any key to quit.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            if (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 0)
            {
                Console.Out.WriteLine("The converter requires Windows XP or newer to run.\nWindows 2000 and older are NOT supported.\n\nPress any key to quit.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            try
            {
                MIDIFile = args[0];
                soundfont = args[1];
                outputFile = args[2];
                voiceLimit = int.Parse(args[3]);
                freq = int.Parse(args[4]);
                if (!File.Exists(MIDIFile))
                {
                    Console.Out.WriteLine("The specified MIDI File doesnt exist!\n\nPress any key to quit.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                if (!File.Exists(soundfont))
                {
                    Console.Out.WriteLine("The specified soundfont File doesnt exist!\n\nPress any key to quit.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                if (File.Exists(outputFile))
                {
                    Console.Out.WriteLine("The specified output file allready exists!\n\nPress any key to quit.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                if(args.Length > 5)
                {
                    for(int i = 5; i < args.Length; i++)
                    {
                        if (args[i].StartsWith("-fx"))
                        {
                            this.useSFX = Boolean.Parse(args[i].Replace("-fx", ""));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.GetBaseException().ToString());
                Console.Out.WriteLine("Error reading arguments!\n\nPress any key to quit.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            try
            {
                    Bass.BASS_Init(0, freq, BASSInit.BASS_DEVICE_NOSPEAKER, IntPtr.Zero);

                    Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 0);
                    Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, 32);
                    Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_MIDI_VOICES, 100000);
                    _recHandle = BassMidi.BASS_MIDI_StreamCreateFile(MIDIFile, 0L, 0L, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_SAMPLE_FX | BASSFlag.BASS_MIDI_DECAYEND, freq);
                    Bass.BASS_ChannelSetAttribute(_recHandle, BASSAttribute.BASS_ATTRIB_MIDI_VOICES, Convert.ToInt32(voiceLimit));
                    Un4seen.Bass.Bass.BASS_ChannelSetAttribute(_recHandle, BASSAttribute.BASS_ATTRIB_MIDI_CPU, 0);
                    Un4seen.Bass.Misc.DSP_PeakLevelMeter _plm = new Un4seen.Bass.Misc.DSP_PeakLevelMeter(_recHandle, 1);
                    _plm.CalcRMS = true;
                    BASS_MIDI_FONT font = new BASS_MIDI_FONT();
                    font.font = BassMidi.BASS_MIDI_FontInit(soundfont);
                    font.preset = -1;
                    font.bank = 0;
                    BASS_MIDI_FONT[] fonts = new BASS_MIDI_FONT[] { font };
                    BassMidi.BASS_MIDI_StreamSetFonts(_recHandle, fonts, 1);
                    BassMidi.BASS_MIDI_StreamLoadSamples(_recHandle);
                    _Encoder = BassEnc.BASS_Encode_Start(_recHandle, outputFile, BASSEncode.BASS_ENCODE_AUTOFREE | BASSEncode.BASS_ENCODE_PCM, null, IntPtr.Zero);
                    if (useSFX)
                    {
                        Un4seen.Bass.Bass.BASS_ChannelFlags(_recHandle, 0, BASSFlag.BASS_MIDI_NOFX);
                    }
                    else {
                    Un4seen.Bass.Bass.BASS_ChannelFlags(_recHandle, BASSFlag.BASS_MIDI_NOFX, BASSFlag.BASS_MIDI_NOFX);
                    }
                    //TODO: Add argument for this
                    Un4seen.Bass.Bass.BASS_ChannelFlags(_recHandle, 0, BASSFlag.BASS_MIDI_NOTEOFF1);
                    int ActiveVoicesInt = 0;
                    int CurrentStatusValueInt = 0;
                    int CurrentStatusMaximumInt = 0;
                    while (Un4seen.Bass.Bass.BASS_ChannelIsActive(_recHandle) == BASSActive.BASS_ACTIVE_PLAYING)
                    {
                            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, 10000);
                            int length = Convert.ToInt32(Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(_recHandle, 0.02));
                            long pos = Un4seen.Bass.Bass.BASS_ChannelGetLength(_recHandle);
                            long num6 = Un4seen.Bass.Bass.BASS_ChannelGetPosition(_recHandle);
                            float num7 = ((float)pos) / 1048576f;
                            float num8 = ((float)num6) / 1048576f;
                            double num9 = Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(_recHandle, pos);
                            double num10 = Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(_recHandle, num6);
                            TimeSpan span = TimeSpan.FromSeconds(num9);
                            TimeSpan span2 = TimeSpan.FromSeconds(num10);
                            string str4 = span.Hours.ToString().PadLeft(2, '0') + ":" + span.Minutes.ToString().PadLeft(2, '0') + ":" + span.Seconds.ToString().PadLeft(2, '0');
                            string str5 = span2.Hours.ToString().PadLeft(2, '0') + ":" + span2.Minutes.ToString().PadLeft(2, '0') + ":" + span2.Seconds.ToString().PadLeft(2, '0');
                            float num11 = 0f;
                            float num12 = 0f;
                            Un4seen.Bass.Bass.BASS_ChannelGetAttribute(_recHandle, BASSAttribute.BASS_ATTRIB_MIDI_VOICES_ACTIVE, ref num11);
                            Un4seen.Bass.Bass.BASS_ChannelGetAttribute(_recHandle, BASSAttribute.BASS_ATTRIB_CPU, ref num12);
                            float[] buffer = new float[length / 4];
                            Un4seen.Bass.Bass.BASS_ChannelGetData(_recHandle, buffer, length);
                            if (num12 < 100f)
                            {
                                Console.Clear();
                                Console.Out.WriteLine(num8.ToString("0.0") + "MBs of RAW datas converted. (Estimated final WAV size: " + num7.ToString("0.0") + "MB)\nCurrent position: " + str5.ToString() + " - " + str4.ToString() + "\nBASS CPU usage: " + Convert.ToInt32(num12).ToString() + "%");
                                Console.Out.WriteLine("Voices: " + ActiveVoicesInt + "/" + voiceLimit);
                                Console.Out.WriteLine("Status: " + CurrentStatusValueInt + "/" + CurrentStatusMaximumInt);
                            }
                            else if (num12 > 100f)
                            {
                                Console.Clear();
                                Console.Out.WriteLine(num8.ToString("0.0") + "MBs of RAW datas converted. (Estimated final WAV size: " + num7.ToString("0.0") + "MB)\nCurrent position: " + str5.ToString() + " - " + str4.ToString() + "\nBASS CPU usage: " + Convert.ToInt32(num12).ToString() + "% (" + ((float)(num12 / 100f)).ToString("0.0") + "x~ more slower)");
                                Console.Out.WriteLine("Voices: " + ActiveVoicesInt + "/" + voiceLimit);
                                Console.Out.WriteLine("Status: " + CurrentStatusValueInt + "/" + CurrentStatusMaximumInt);
                            }
                            ActiveVoicesInt = Convert.ToInt32(num11);
                            CurrentStatusMaximumInt = Convert.ToInt32((long)(pos / 0x100000L));
                            CurrentStatusValueInt = Convert.ToInt32((long)(num6 / 0x100000L));
                    }
                Console.Out.WriteLine("Conversion finished\n\nPress any key to quit.");
                Console.ReadKey();
                BassEnc.BASS_Encode_Stop(_Encoder);
                Bass.BASS_StreamFree(_recHandle);
                Bass.BASS_Free();
            }
            catch(Exception e)
            {
                BassEnc.BASS_Encode_Stop(_Encoder);
                Bass.BASS_StreamFree(_recHandle);
                Bass.BASS_Free();
                Console.Out.WriteLine(e.ToString());
                Console.Out.WriteLine("Conversion failed!\n\nPress any key to quit.");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

    }
}
