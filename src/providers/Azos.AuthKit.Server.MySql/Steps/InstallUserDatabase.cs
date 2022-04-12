﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Conf;
using Azos.Platform;
using Azos.Scripting.Steps;
using Azos.Serialization.JSON;

using MySqlConnector;

namespace Azos.AuthKit.Server.MySql.Steps
{
  public sealed class InstallUserDatabase : EntryPoint
  {
    public InstallUserDatabase(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config]
    public string MySqlConnectString { get; set; }

    [Config]
    public string DbName { get; set; }

    [Config]
    public bool SkipDbCreation {  get ; set; }

    [Config]
    public bool SkipDdl { get; set; }

    protected override string DoRun(JsonDataMap state)
    {
      var rel = Guid.NewGuid();
      var cs = MySqlConnectString.NonBlank(nameof(MySqlConnectString));
      using(var cnn = new MySqlConnection(cs))
      {
        cnn.Open();
        doConnectionWork(cnn, rel);
      }

      return null;
    }

    private void doConnectionWork(MySqlConnection cnn, Guid rel)
    {
      using(var cmd = cnn.CreateCommand())
      {
        //Step 1. Create database
        if (!SkipDbCreation)
        {
          WriteLog(Log.MessageType.Info, nameof(doConnectionWork), "Will create database", related: rel);
          createDatabase(cmd, rel);
          WriteLog(Log.MessageType.Info, nameof(doConnectionWork), "Db created", related: rel);
        }

        //Step 2. Create DDL
        if (!SkipDdl)
        {
          WriteLog(Log.MessageType.Info, nameof(doConnectionWork), "Will run DDL", related: rel);
          createDdl(cmd, rel);
          WriteLog(Log.MessageType.Info, nameof(doConnectionWork), "DDL ran", related: rel);
        }
      }
    }

    private void createDatabase(MySqlCommand cmd, Guid rel)
    {
      var ddl = typeof(MySqlUserStore).GetText("ddl.db_ddl.sql");
      var dbn = DbName.NonBlankMinMax(5, 32, nameof(DbName));
      WriteLog(Log.MessageType.Info, nameof(createDatabase), "Db is: {0}".Args(dbn), related: rel);

      ddl = ddl.Args(dbn);
      cmd.CommandText = ddl;
      WriteLog(Log.MessageType.Info, nameof(createDatabase), "Starting cmd exec...", related: rel, pars: ddl);

      sql(cmd, nameof(createDatabase), rel);
    }

    private void createDdl(MySqlCommand cmd, Guid rel)
    {
      var dbn = DbName.NonBlankMinMax(5, 32, nameof(DbName));
      WriteLog(Log.MessageType.Info, nameof(createDatabase), "Set db to: {0}".Args(dbn), related: rel);
      var ddl = "use `{0}`";
      WriteLog(Log.MessageType.Info, nameof(createDatabase), "Starting cmd exec...", related: rel, pars: ddl);
      cmd.CommandText = ddl;
      sql(cmd, nameof(createDdl), rel);


      ddl = typeof(MySqlUserStore).GetText("ddl.user_ddl.sql");
      cmd.CommandText = ddl;
      sql(cmd, nameof(createDdl), rel);
    }

    private void sql(MySqlCommand cmd,  string from, Guid rel)
    {
      try
      {
        cmd.ExecuteNonQuery();
        WriteLog(Log.MessageType.Info, from, "...Done", related: rel);
      }
      catch (Exception error)
      {
        WriteLog(Log.MessageType.CatastrophicError, from, "Error: {0}".Args(error.ToMessageWithType()), related: rel, error: error);
        throw;
      }
    }


  }
}
