using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Domain.Processor;

namespace Domain
{
    public class JaunteeProcessor : BaseProcessor
    {
        public JaunteeProcessor( ProcessorContext context ) : base( context ) { }

        public override ProcessorType CanProcessType {
            get { return ProcessorType.Jauntee; }
        }

        protected override bool RenameDuplicateFileNames { get { return false; } }

        //If you want to still copy the files if files are missing
        protected override bool AllowedToProceedIfFilesMissing { get { return true; } }

        //Used for jauntee and gets 120/132
        protected override Regex ParseFileNamesRegex { get { return new Regex( "(file:)+((.*?)(flac.*?(mp3|flac)))" ); } }
    }
}
