using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wkhtmltopdf.NetCore.Options;
using Wkhtmltopdf.NetCore;

namespace VC.AG.Models.ValuesObject
{
    public class PdfConvertOptions : ConvertOptions
    {
        [OptionFlag("--footer-center")]
        public string? FooterCenter { get; set; }
    }
}
