using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Domain.Processor;
using Domain;

namespace Foobar2000PlaylistCopier
{
    public class Program
    {
        public static void Main( string[] args ) {
            ///IMPORTANT: Make sure to always make a copy of the playlist.  It might destroy the playlist file!
            var context = new ProcessorContext {
                FileName = "C:\\Users\\Dan\\Music\\Playlists\\Heater Jauntee - Copy.fpl",
                Destination = @"C:\Users\Dan\Music\Playlists\Heater Jauntee 8_3_15\"
            };

            ProcessorType fileType = context.FileName.Contains( "Phish" ) ? ProcessorType.Phish : ProcessorType.Jauntee;

            IEnumerable<IProcess> processors = new IProcess[] { new JaunteeProcessor( context ), new PhishProcessor( context ) };

            IProcess chosenProcessor = null;
            foreach(var processor in processors){
                if(processor.CanProcessType == fileType){
                    chosenProcessor = processor;
                    break;
                }
            }
            var success = chosenProcessor != null ? chosenProcessor.Process() : false;

            Console.WriteLine( "\r\n Finished Processing.  Press a key to proceed." );
            Console.ReadKey();
            Environment.Exit( success ? 1 : 0 );
        }
    }
}
