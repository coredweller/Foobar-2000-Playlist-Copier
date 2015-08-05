using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Domain.Processor
{
    public interface IProcess
    {
        bool Process();
        ProcessorType CanProcessType { get; }
    }

    public abstract class BaseProcessor : IProcess
    {
        private readonly ProcessorContext _context;
        private IList<string> _fileNames;

        public BaseProcessor( ProcessorContext context ) {
            _context = context;
        }

        public abstract ProcessorType CanProcessType { get; }

        //If there is a file already in the folder we are copying to.  Decide whether you want to discard the second one or change the name and copy it.
        protected virtual bool RenameDuplicateFileNames { get { return true; } }

        //If you want to still copy the files if files are missing
        protected virtual bool AllowedToProceedIfFilesMissing { get { return false; } }

        //The Regex used to parse the File Names out of the playlist
        protected virtual Regex ParseFileNamesRegex { get { return new Regex( "(file:)+((.*?)(mp3|flac))" ); } }

        //The Regex used to parse the song names from the files
        protected virtual Regex ParseSongNameRegex { get { return new Regex( @"(\\)+(?:.(?!\\))+$" ); } }

        public bool Process() {
            return CheckFileStatus() ? CopyFiles( _fileNames, _context.Destination ) : false;
        }

        private bool CheckFileStatus() {

            //Get all file names from the file
            _fileNames = GetFileNames( _context.FileName ).ToList();
            if ( _fileNames == null || !_fileNames.Any() ) {
                Console.WriteLine( "File Names are null, exiting out" );
                return false;
            }

            Console.WriteLine("File Names Found: {0}", _fileNames.Count());

            //Check to make sure that they all exist and can be found
            var filesThatDontExist = DoAllFilesExist( _fileNames );
            if ( filesThatDontExist != null && filesThatDontExist.Any() ) {
                Console.WriteLine( "Files that were not found: " );

                foreach ( var dontExist in filesThatDontExist ) {
                    Console.WriteLine( dontExist + "\r\n" );
                    _fileNames.Remove(dontExist);
                }

                return AllowedToProceedIfFilesMissing;
            }

            Console.WriteLine( "Files that exist: {0}", _fileNames.Count() );

            return true;
        }

        //Copy files from source location to new destination
        private bool CopyFiles( IEnumerable<string> fileNames, string destination ) {
            var successfulCount = 0;
            try {
                foreach ( var file in fileNames ) {
                    
                    try {
                        Match match = ParseSongNameRegex.Match( file );
                        if ( match.Success ) {
                            var name = match.ToString().Remove( 0, 1 );
                            var finalDestination = destination + name;

                            var exists = false;
                            if ( File.Exists( finalDestination ) ) {
                                if ( RenameDuplicateFileNames ) {
                                    Random r = new Random();
                                    var randomNumber = r.Next( 2, 1000 );
                                    name = randomNumber + name;
                                    finalDestination = destination + name;
                                }
                                exists = true;
                            }

                            if ( !exists || ( exists && RenameDuplicateFileNames ) ) {
                                Console.WriteLine( "About to copy: " + finalDestination );
                                File.Copy( file, finalDestination );
                                Console.WriteLine( "Successfully copied: " + finalDestination );
                            }
                            else {
                                Console.WriteLine( "We will not copy {0} because it exists and this processor type does not allow renaming of duplicate file names", file );
                            }

                            successfulCount++;
                        }
                        else {
                            Console.WriteLine( "File did not get copied: " + file );
                        }
                    }
                    catch(ArgumentException ae)
                    {
                        Console.WriteLine( "Exception copying file {0} for reason {1}", file, ae.Message );
                    }
                }

                return true;
            }
            catch ( Exception ex ) {
                Console.WriteLine( "EXCEPTION OCCURRED in CopyFiles: " + ex.Message );
                Console.WriteLine( "We made it through {0} out of {1} successfully before it failed", successfulCount, fileNames.Count() );
                return false;
            }
        }

        //Returns all file names that do not exist
        private IEnumerable<string> DoAllFilesExist( IEnumerable<string> fileNames ) {
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
        private IEnumerable<string> GetFileNames( string fileName ) {
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
