using System.Collections;
using System.Collections.Generic;

namespace DatabaseDumper.Models
{
    public class TableInfo
    {
        public string TableCatalog { get; set; }
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string IsInsertableInto { get; set; }
        public string IsTyped { get; set; }

        public bool IsInsertableIntoValue => IsInsertableInto == "YES";
        public bool IsTypedValue => IsTyped == "YES";
        public IEnumerable<ColumnInfo> Columns { get; set; }
        public IEnumerable<TableInfo> ReferencedBy { get; set; }
        public IEnumerable<TableInfo> References { get; set; }
    }
}