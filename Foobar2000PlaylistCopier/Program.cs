using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Foobar2000PlaylistCopier
{
    public class Program
    {
        private static readonly Regex ParseFileNamesRegex = new Regex( "(file:)+((.*?)(mp3|flac))" );
        private static readonly Regex ParseSongNameRegex = new Regex( @"(\\)+(?:.(?!\\))+$" );

        static void Main( string[] args ) {

            var fileName = "C:\\Users\\Dan\\Music\\Playlists\\Heater Phish - Copy.fpl";
            var destination = @"C:\Users\Dan\Music\Playlists\destination\";

            //Get all file names from the file
            var fileNames = GetFileNames( fileName );
            if ( fileNames == null ) {
                Console.WriteLine( "File Names are null, exiting out" );
                Environment.Exit( 0 );
            }

            //Check to make sure that they all exist and can be found
            var filesThatDontExist = DoAllFilesExist( fileNames );
            if ( filesThatDontExist != null && filesThatDontExist.Any() ) {
                Console.WriteLine( "Files that were not found: " );

                foreach ( var dontExist in filesThatDontExist ) {
                    Console.WriteLine( dontExist );
                }

                Environment.Exit( 0 );
            }

            //Copy files from source location to new destination
            var success = CopyFiles( fileNames, destination );

            Console.ReadKey();
            Environment.Exit( success ? 1 : 0 );
        }

        //Copy files from source location to new destination
        private static bool CopyFiles( IEnumerable<string> fileNames, string destination ) {
            try {

                var count = 0;
                foreach ( var file in fileNames ) {
                    count++;
                    Match match = ParseSongNameRegex.Match( file );
                    if ( match.Success ) {
                        var name = match.ToString().Remove( 0, 1 );
                        var finalDestination = destination + name;

                        if ( File.Exists( finalDestination ) ) {
                            Random r = new Random();
                            var randomNumber = r.Next( 2, 1000 );
                            name = randomNumber + name;
                            finalDestination = destination + name;
                        }

                        File.Copy( file, finalDestination);
                    }
                    else {
                        Console.WriteLine( "File did not get copied: " + file );
                    }
                }

                return true;
            }
            catch ( Exception ex ) {
                Console.WriteLine( "EXCEPTION OCCURRED in CopyFiles: " + ex.Message );
                return false;
            }
        }

        //Returns all file names that do not exist
        private static IEnumerable<string> DoAllFilesExist( IEnumerable<string> fileNames ) {
            var doNotExistList = new List<string>();

            try {
                foreach ( var f in fileNames ) {
                    if ( !File.Exists( f ) ) {
                        doNotExistList.Add( f );
                    }
                }
            }
            catch ( Exception ex ) {
                Console.WriteLine( "EXCEPTION OCCURRED in DoAllFilesExist: " + ex.Message );
                return null;
            }

            return doNotExistList;
        }

        //Gets all filenames from a play list file
        private static IEnumerable<string> GetFileNames( string fileName ) {
            var fileNames = new List<string>();

            try {
                using ( StreamReader streamReader = new StreamReader( fileName ) ) {
                    var contents = streamReader.ReadToEnd();

                    var found = true;
                    while ( found ) {
                        Match match = ParseFileNamesRegex.Match( contents );
                        if ( match.Success ) {
                            fileNames.Add( match.ToString().Replace( "file://", string.Empty ) );
                            contents = contents.Replace( match.ToString(), string.Empty );
                        }
                        else {
                            found = false;
                        }
                    }
                }
            }
            catch ( Exception ex ) {
                Console.WriteLine( "EXCEPTION OCCURRED in GetFileNames: " + ex.Message );
                return null;
            }

            return fileNames;
        }
    }
}
