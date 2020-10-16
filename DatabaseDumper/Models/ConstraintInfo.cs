using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseDumper.Models
{
    public class ConstraintInfo
    {
        private static Regex foreignKeyExpression = new Regex(@"FOREIGN\sKEY\s\((?<KeyName>\w*)\)\sREFERENCES\s(?<ReferenceTable>\w*)\((?<ReferenceColumn>\w*)");
        public string TableFrom { get; set; }
        public string ConstraintName { get; set; }
        public string Constraint { get; set; }
        public ForeignKey ForeignKey
        {
            get
            {
                var matches = foreignKeyExpression.Matches(Constraint);
                if (!matches.Any())
                    return null;
                return new ForeignKey()
                {
                    KeyName = matches.First().Groups["KeyName"].Value,
                    ReferencedTable = matches.First().Groups["ReferenceTable"].Value,
                    ReferenceColumn = matches.First().Groups["ReferenceColumn"].Value,
                };
            }
        }

    }

    public class ForeignKey
    {
        public string KeyName { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferenceColumn { get; set; }
    }
}
