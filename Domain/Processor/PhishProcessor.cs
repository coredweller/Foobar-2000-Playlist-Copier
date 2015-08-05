using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Domain.Processor
{
    public class PhishProcessor : BaseProcessor
    {
        public PhishProcessor( ProcessorContext context) : base( context ) { }

        public override ProcessorType CanProcessType {
            get { return ProcessorType.Phish; }
        }
    }
}
