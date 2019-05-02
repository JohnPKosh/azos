﻿using System;
using System.Collections.Generic;
using System.Text;
using Azos.Data;
using Azos.Scripting;
using Azos.Security;
using Azos.Wave.Mvc;

namespace Azos.Tests.Unit.Wave
{
  [Runnable]
  public class ApiDocGenTests
  {
    [Run]
    public void Gen1()
    {
      var gen = new ApiDocGenerator();
      gen.Locations.Add(new ApiDocGenerator.ControllerLocation("Azos.Tests.Unit.dll", "Azos.Tests.Unit.Wave*"));

      var got = gen.Generate();

      Console.WriteLine( got.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint) );
    }
  }

  public class GoodPersonPermission : TypedPermission
  {
    public GoodPersonPermission(int level): base(level) { }

    public override string Description
      => "This permission requires the caller to be a good person of minimum level of {0}. Notice this text comes from instance, not type".Args(Level);
  }


  public class PersonName : TypedDoc
  {
    [Field] public string FirstName { get; set; }
    [Field] public string LastName { get; set; }
  }

  public class TestFilter : TypedDoc
  {
    [Field] public string StringField  { get;set; }
    [Field] public PersonName PersonNameDocField   { get; set; }

    [Field(valueList: "key1: description1, key2: description2")]
    public string PickField { get; set; }

    [Field(required: true, min: -100, max: 1000, dflt: 9000, displayFormat:"2n",  formatDescr: "my format description")]
    public object ObjectField { get; set; }

    [Field] public byte ByteField { get; set; }
    [Field] public sbyte SByteField { get; set; }
    [Field] public int IntField      { get; set; }
    [Field] public uint UIntField    { get; set; }
    [Field] public short ShortField  { get; set; }
    [Field] public ushort UshortField { get; set; }
    [Field] public long LongField { get; set; }
    [Field] public ulong ULongField { get; set; }
    [Field] public float FloatField { get; set; }
    [Field] public double DoubleField { get; set; }
    [Field] public decimal DecimalField { get; set; }
    [Field] public bool BoolField { get; set; }

    [Field] public int[] IntArrayField { get; set; }
    [Field] public int?[] NIntArrayField { get; set; }
    [Field] public decimal[] DecimalArrayField { get; set; }
    [Field] public decimal?[] NDecimalArrayField { get; set; }

    [Field] public string[] StringArrayField { get; set; }

    [Field] public object[] ArrayOfObjectField { get; set; }
    [Field] public List<object> ListOfObjectField { get; set; }

    [Field] public TestFilter[] ArrayOfDocField { get; set; }
    [Field] public List<TestFilter> ListOfDocField { get; set; }

    [Field] public Dictionary<string, List<TestFilter[]>> DictionaryOfStringDocField { get; set; }
  }


  [ApiControllerDoc(Title ="TestBase", BaseUri = "/test", RequestBody ="json or form url encoded", ResponseHeaders =new[]{"Cache: no-cache"})]
  public class TestController : Controller
  {
    [GoodPersonPermission(1), GoodPersonPermission(121)]
    [ApiEndpointDoc(Title="Get list schema", Methods =new[]{ "GET: Gets the schema"}, TypeSchemas = new[] { typeof(TestController) })]
    [Action(Name ="list")]
    public object ListGet(TestFilter filter)
    {
      return null;
    }

    [ApiEndpointDoc(
      Uri="manual-list",
      Title = "Post filter content returning filtered data list",
      Description ="Lorem ipsum doldol buldum han if shak zum ser content and it nicht sein bina fur euch davon ich hat",
      Methods = new[] { "POST: post filter body and generates json with data" },
      TypeSchemas = new[] { typeof(TestController) }
      )]
    [Action(Name = "list")]
    public object ListPost()
    {
      return null;
    }

    [ApiEndpointDoc]
    [ActionOnGet(Name = "something-custom-name")]
    public object SomeAction()
    {
      return null;
    }
  }


}
