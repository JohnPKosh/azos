﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Scripting.Steps
{
  /// <summary>
  /// Sets a global or local value to the specified expression
  /// </summary>
  /// <example><code>
  ///  do{ type="Set" global=a to='((x * global.a) / global.b) + 23'}
  ///  do{ type="Set" local=x to='x+1' name="inc x"}
  /// </code></example>
  public sealed class Set : Step
  {
    public const string CONFIG_TO_ATTR = "to";
    public const string CONFIG_GLOBAL_ATTR = "global";
    public const string CONFIG_LOCAL_ATTR = "local";

    public const string GLOBAL = "global";
    public const string UNKNOWN = "unknown";


    public Set(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx)
    {
      var toExpression = cfg.ValOf(CONFIG_TO_ATTR).NonBlank(CONFIG_TO_ATTR);
      m_Eval = new Text.Evaluator(toExpression);

      m_Local = cfg.ValOf(CONFIG_LOCAL_ATTR);
      m_Global = cfg.ValOf(CONFIG_GLOBAL_ATTR);

      if (m_Local.IsNullOrWhiteSpace() && m_Global.IsNullOrWhiteSpace())
      {
        throw new RunnerException("Set step requires at least either global or local assignment");
      }
    }

    private Text.Evaluator m_Eval;
    private string m_Global;
    private string m_Local;

    protected override string DoRun(JsonDataMap state)
    {
      var got = m_Eval.Evaluate(id => GetResolver(Runner, id, state));

      if (m_Global.IsNotNullOrWhiteSpace())
      {
        Runner.GlobalState[m_Global] = got;
      }

      if (m_Local.IsNotNullOrWhiteSpace())
      {
        state[m_Local] = got;
      }

      return null;
    }

    /// <summary>
    /// Resolves `global.x` to `Runner.Globals[x]` otherwise to `state[x]`
    /// </summary>
    public static string GetResolver(StepRunner runner, string ident, JsonDataMap state)
    {
      if (ident.IsNullOrWhiteSpace()) return ident;
     // Conout.SeeArgs("Ident: {0}", ident);
     // Conout.See(runner.GlobalState);
      if (double.TryParse(ident, out var _)) return ident;

      var pair = ident.SplitKVP('.');
      if (pair.Key == GLOBAL)
        return runner.GlobalState[pair.Value.Default(UNKNOWN)].AsString();
      else
        return state[ident].AsString();
    }
  }

}