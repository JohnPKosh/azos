
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Data;


namespace Azos.Serialization.CSV
{
  public static class CSVWriter
  {
    public static void WriteToFile(RowsetBase rowset,
                                   string fileName,
                                   CSVWritingOptions options = null,
                                   Encoding encoding = null,
                                   FieldFilterFunc filter = null)
    {
      using(var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
          Write(rowset, fs, options, encoding, filter);
    }

    public static byte[] WriteToBuffer(RowsetBase rowset,
                                       CSVWritingOptions options = null,
                                       Encoding encoding = null,
                                       FieldFilterFunc filter = null)
    {
      using(var ms = new MemoryStream())
      {
        Write(rowset, ms, options, encoding, filter);
        return ms.ToArray();
      }
    }

    public static void Write(RowsetBase rowset,
                             Stream stream,
                             CSVWritingOptions options = null,
                             Encoding encoding = null,
                             FieldFilterFunc filter = null)
    {
      using (var wri = new StreamWriter(stream, encoding ?? UTF8Encoding.UTF8))
      {
        Write(rowset, wri, options, filter);
      }
    }

    public static string Write(RowsetBase rowset,
                               CSVWritingOptions options = null,
                               FieldFilterFunc filter = null)
    {
      var sb = new StringBuilder();
      using(var wri = new StringWriter(sb))
      {
        Write(rowset, wri, options, filter);
      }

      return sb.ToString();
    }

    public static void Write(RowsetBase rowset,
                             TextWriter wri,
                             CSVWritingOptions options = null,
                             FieldFilterFunc filter = null)
    {
      if (rowset == null) return;
      if (options == null) options = CSVWritingOptions.Default;

      var defs = getAcceptableDefs(rowset.Schema, options.LoadAllFields, filter);

      if (options.IncludeHeader) writeHeader(defs, wri, options);

      foreach(var item in rowset.AsReadonlyIList)
      {
        var doc = item as Doc;
        if (doc == null) continue;

        writeRow(doc, defs, wri, options);
      }
    }

    public static void WriteToFile(Doc doc,
                                   string fileName,
                                   CSVWritingOptions options = null,
                                   Encoding encoding = null,
                                   FieldFilterFunc filter = null)
    {
      using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
      {
        Write(doc, fs, options, encoding, filter);
      }
    }

    public static byte[] WriteToBuffer(Doc doc,
                                       CSVWritingOptions options = null,
                                       Encoding encoding = null,
                                       FieldFilterFunc filter = null)
    {
      using(var ms = new MemoryStream())
      {
        Write(doc, ms, options, encoding, filter);
        return ms.ToArray();
      }
    }

    public static string Write(Doc doc,
                               CSVWritingOptions options = null,
                               FieldFilterFunc filter = null)
    {
      var sb = new StringBuilder();
      using(var wri = new StringWriter(sb))
      {
        Write(doc, wri, options, filter);
      }

      return sb.ToString();
    }

    public static void Write(Doc doc,
                             Stream stream,
                             CSVWritingOptions options = null,
                             Encoding encoding = null,
                             FieldFilterFunc filter = null)
    {
      using (var wri = new StreamWriter(stream, encoding ?? Encoding.UTF8))
      {
        Write(doc, wri, options, filter);
      }
    }

    public static void Write(Doc doc,
                             TextWriter wri,
                             CSVWritingOptions options = null,
                             FieldFilterFunc filter = null)
    {
      if (doc == null) return;
      if (options == null) options = CSVWritingOptions.Default;

      var defs = getAcceptableDefs(doc.Schema, options.LoadAllFields, filter);

      if (options.IncludeHeader) writeHeader(defs, wri, options);

      writeRow(doc, defs, wri, options);
    }

    #region .pvt
      private static IEnumerable<Schema.FieldDef> getAcceptableDefs(Schema schema, bool allFields, FieldFilterFunc filter)
      {
        if (schema == null)
          throw new AzosSerializationException("CSVWriter.getAcceptableDefs(schema=null)");

        filter = filter ?? ((r, k, fd) => true);
        var filteredFields = schema.FieldDefs.Where(fd => filter(null, null, fd));

        if (allFields) return filteredFields;

        var res = new List<Schema.FieldDef>();

        foreach(var def in filteredFields)
        {
          if (!filter(null, null, def)) continue;
          if (!isWritable(def)) continue;
          res.Add(def);
        }

        return res;
      }

      private static void writeHeader(IEnumerable<Schema.FieldDef> defs, TextWriter wri, CSVWritingOptions options)
      {
        var first = true;
        var newline = false;
        foreach(var def in defs)
        {
          if (!first) wri.Write(options.FieldDelimiter);
          wri.Write(def.Name);

          first = false;
          newline = true;
        }

        if (newline) wri.WriteLine();
      }

      private static void writeRow(Doc doc, IEnumerable<Schema.FieldDef> defs, TextWriter wri, CSVWritingOptions options)
      {
        var first = true;
        var newline = false;
        foreach(var def in defs)
        {
          if (!first) wri.Write(options.FieldDelimiter);
          var val = doc.GetFieldValue(def);
          writeValue(val, wri, options);

          first = false;
          newline = true;
        }

        if (newline) wri.WriteLine();
      }

      private static bool isWritable(Schema.FieldDef def)
      {
        var attr = def[null];
        return !(attr.NonUI ||
                 attr.StoreFlag == StoreFlag.None ||
                 attr.StoreFlag == StoreFlag.OnlyLoad);
      }

      private static void writeValue(object value, TextWriter wri, CSVWritingOptions options)
      {
        if (value == null)
        {
          wri.Write(options.NullValue);
          return;
        }

        if (value is string)
        {
          wri.Write(escape((string)value, options.FieldDelimiter));
          return;
        }

        if (value is bool)
        {
          wri.Write(((bool)value) ? "true" : "false");
          return;
        }

        if (value is int || value is long)
        {
          wri.Write(((IConvertible)value).ToString(System.Globalization.CultureInfo.InvariantCulture));
          return;
        }

        if (value is double || value is float || value is decimal)
        {
          wri.Write(escape(((IConvertible)value).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    options.FieldDelimiter));
          return;
        }

        if (value is DateTime)
        {
          wri.Write(((DateTime)value).ToString(System.Globalization.CultureInfo.InvariantCulture));
          return;
        }

        if (value is TimeSpan)
        {
          var ts = (TimeSpan)value;
          wri.Write(ts.Ticks);
          return;
        }

        wri.Write(escape(value.ToString(), options.FieldDelimiter));
      }

      private static string escape(string str, char del)
      {
        bool needsQuotes = str.IndexOfAny(new char[] {'\n', '\r', '"', del}) >= 0;

        str = str.Replace("\"", "\"\"");

        return needsQuotes ? "\"" + str + "\"" : str;
      }
    #endregion
  }
}
