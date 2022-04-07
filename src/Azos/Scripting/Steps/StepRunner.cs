﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Collections;
using Azos.Conf;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.Scripting.Steps
{
  /// <summary>
  /// Facilitates invocation of C# Steps from a script file in sequence.
  /// You can extend this class to supply extra use-case context-specific fields/props.
  /// This class is not thread-safe by design
  /// </summary>
  public class StepRunner
  {
    public const string CONFIG_STEP_SECTION = "step";
    public const double DEFAULT_TIMEOUT_SEC = 60.0d;


    public sealed class HaltSignal : Exception { }

    public StepRunner(IApplication app, IConfigSectionNode rootSource)
    {
      m_App = app.NonNull(nameof(app));
      m_RootSource = rootSource.NonEmpty(nameof(rootSource));
      ConfigAttribute.Apply(this, m_RootSource);
    }

    private IApplication m_App;
    private JsonDataMap m_GlobalState = new JsonDataMap(true);
    private IConfigSectionNode m_RootSource;

    /// <summary>
    /// Application context that this runner operates under
    /// </summary>
    public IApplication App => m_App;


    [Config]
    public virtual double TimeoutSec {  get; set; }

    /// <summary>
    /// Defines log Level
    /// </summary>
    [Config]
    public virtual Azos.Log.MessageType LogLevel {  get; set; }

    /// <summary>
    /// Runner global state, does not get reset between runs (unless you re-set it by step)
    /// </summary>
    public JsonDataMap GlobalState => m_GlobalState;


    /// <summary>
    /// Executes the whole script. The <see cref="GlobalState"/> is NOT cleared automatically.
    /// Returns local state JsonDataMap (private to this run invocation)
    /// </summary>
    /// <param name="ep">EntryPoint instance</param>
    public virtual JsonDataMap Run(EntryPoint ep)
    {
      return this.Run(ep.NonNull(nameof(ep)).Name);
    }

    /// <summary>
    /// Executes the whole script. The <see cref="GlobalState"/> is NOT cleared automatically.
    /// Returns local state JsonDataMap (private to this run invocation)
    /// </summary>
    /// <param name="entryPointStep">Name of step to start execution at, null by default - starts from the very first step</param>
    public virtual JsonDataMap Run(string entryPointStep = null)
    {
      Exception error = null;
      JsonDataMap state = null;
      try
      {
        state = DoBeforeRun();

        OrderedRegistry<Step> script = new OrderedRegistry<Step>();

        Steps.ForEach(s => {

          var added = script.Register(s);

          if (!added) throw new RunnerException($"Duplicate runnable script step `{s.Name}` at '{s.Config.RootPath}'");
        });

        var time = Timeter.StartNew();
        var secTimeout = TimeoutSec;
        if (secTimeout <= 0.0) secTimeout = DEFAULT_TIMEOUT_SEC;

        var ip = 0;
        if (entryPointStep.IsNotNullOrWhiteSpace())
        {
          var ep = script[entryPointStep];
          if (ep==null) throw new RunnerException($"Entry point step `{entryPointStep}` was not found");
          if (!(ep is EntryPoint)) throw new RunnerException($"Entry point step `{entryPointStep}` is not of a valid EntryPoint type");
          ip = ep.Order;
        }

        while(ip < script.Count)
        {
          if (time.ElapsedSec > secTimeout)
          {
            throw new RunnerException("Timeout at {0} sec on [{1}]".Args(time.ElapsedSec, ip));
          }

          var step = script[ip];

          //----------------------------
          var nextStepName = step.Run(state); //<----------- RUN
          //----------------------------

          if (nextStepName.IsNullOrWhiteSpace())
          {
            ip++;
          }
          else
          {
            var next = script[nextStepName];
            if (next==null) throw new RunnerException($"Step not found: `{nextStepName}` by {step}");
            ip = next.Order;
          }
        }//for
      }
      catch(Exception cause)
      {
        if (!(cause is HaltSignal))
        {
          error = cause;
        }
      }

      var handled = DoAfterRun(error, state);
      if (!handled && error != null) throw error;

      return state;
    }

    /// <summary>
    /// Invoked before steps, makes run state instance.
    /// Default implementation makes case-sensitive state bag
    /// </summary>
    protected virtual JsonDataMap DoBeforeRun()
    {
      return new JsonDataMap(true);
    }

    /// <summary>
    /// Invoked after all steps are run, if error is present it is set and return true if
    /// you handle the error yourself, otherwise return false for default processing
    /// </summary>
    protected virtual bool DoAfterRun(Exception error, JsonDataMap state)
    {
      return false;
    }

    /// <summary>
    /// Returns all runnable steps, default implementation returns all sections named "STEP"
    /// in their declaration syntax
    /// </summary>
    public virtual IEnumerable<IConfigSectionNode> StepSections
      => m_RootSource.ChildrenNamed(CONFIG_STEP_SECTION);

    /// <summary>
    /// Returns materialized steps of <see cref="StepSections"/>
    /// </summary>
    public virtual IEnumerable<Step> Steps
    {
      get
      {
        var i = 0;
        foreach(var nstep in StepSections)
        {
          var step = FactoryUtils.Make<Step>(nstep, null, new object[] { this, nstep, i });
          yield return step;
          i++;
        }
      }
    }

    /// <summary>
    /// Returns explicit entry point names
    /// </summary>
    public IEnumerable<EntryPoint> EntryPoints => Steps.OfType<EntryPoint>();

    /// <summary>
    /// Writes a log message for this runner; returns the new log msg GDID for correlation, or GDID.Empty if no message was logged.
    /// The file/src are only used if `from` is null/blank
    /// </summary>
    protected internal virtual Guid WriteLog(Azos.Log.MessageType type,
                                               string from,
                                               string text,
                                               Exception error = null,
                                               Guid? related = null,
                                               string pars = null,
                                               [System.Runtime.CompilerServices.CallerFilePath]string file = null,
                                               [System.Runtime.CompilerServices.CallerLineNumber]int src = 0)
    {
      if (type < LogLevel) return Guid.Empty;

      if (from.IsNullOrWhiteSpace())
        from = "{0}:{1}".Args(file.IsNotNullOrWhiteSpace() ? System.IO.Path.GetFileName(file) : "?", src);

      var msg = new Azos.Log.Message
      {
        App = App.AppId,
        Topic = CoreConsts.RUN_TOPIC,
        From = "{0}.{1}".Args(GetType().DisplayNameWithExpandedGenericArgs(), from),
        Type = type,
        Text = text,
        Exception = error,
        Parameters = pars,
        Source = src
      };

      msg.InitDefaultFields(App);

      if (related.HasValue) msg.RelatedTo = related.Value;

      App.Log.Write(msg);

      return msg.Guid;
    }

  }
}
