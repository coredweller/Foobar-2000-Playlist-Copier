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
        //Used for phish and worked perfectly
        //private static readonly Regex ParseFileNamesRegex = new Regex( "(file:)+((.*?)(mp3|flac))" );

        //Used for jauntee and gets 120/132
        private static readonly Regex ParseFileNamesRegex = new Regex( "(file:)+((.*?)(flac.*?(mp3|flac)))" );
        
        private static readonly Regex ParseSongNameRegex = new Regex( @"(\\)+(?:.(?!\\))+$" );

        static void Main( string[] args ) {
            ///IMPORTANT: Make sure to always make a copy of the playlist.  It might destroys the playlist file!
            var fileName = "C:\\Users\\Dan\\Music\\Playlists\\Heater Jauntee - Copy.fpl";
            var destination = @"C:\Users\Dan\Music\Playlists\Heater Jauntee 8_3_15\";
            var success = true;

            //Get all file names from the file
            var fileNames = GetFileNames( fileName );
            if ( fileNames == null ) {
                Console.WriteLine( "File Names are null, exiting out" );
                success = false;
            }

            //Check to make sure that they all exist and can be found
            var filesThatDontExist = DoAllFilesExist( fileNames );
            if ( filesThatDontExist != null && filesThatDontExist.Any() ) {
                Console.WriteLine( "Files that were not found: " );

                foreach ( var dontExist in filesThatDontExist ) {
                    Console.WriteLine( dontExist );
                }

                success = false;
            }
            
            //Copy files from source location to new destination
            success = success ? CopyFiles( fileNames, destination ) : false;

            Console.WriteLine();
            Console.WriteLine( "Finished Copying files" );
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

                        Console.WriteLine( "About to copy: " + finalDestination );
                        File.Copy( file, finalDestination);
                        Console.WriteLine( "Successfully copied: " + finalDestination );
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
