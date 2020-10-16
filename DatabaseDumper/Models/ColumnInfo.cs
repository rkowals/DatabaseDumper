using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseDumper.Models
{
    public class ColumnInfo
    {
        public string TableCatalog { get; set; }
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int OrdinalPosition { get; set; }
        public string IsNullable { get; set; }
        public string DataType { get; set; }
        public string IsIdentity { get; set; }
        public string IsGenerated { get; set; }
        public string IsUpdatable { get; set; }

        public bool IsNullableValue => IsNullable == "YES";
        public bool IsIdentityValue => IsIdentity == "YES";
        public bool IsGeneratedValue => IsGenerated == "YES";
        public bool IsUpdatableValue => IsUpdatable == "YES";
    }
}
